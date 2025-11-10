using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace IpoDataDartTest
{
    public class clsDartCollector
    {
        private readonly string _dartKey;
        private readonly HttpClient _client;

        public clsDartCollector(string dartApiKey)
        {
            _dartKey = dartApiKey.Trim();
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        }

        /// <summary>
        /// 공모주 청약정보 (DART + KRX API 통합)
        /// </summary>
        public async Task<List<clsIPOData>> CollectIPOData(string startDate, string endDate)
        {
            var result = new List<clsIPOData>();

            // 1️⃣ DART: 최근 증권신고서(지분증권)
            var dartList = await GetDartIPOListAsync(startDate, endDate);

            // 2️⃣ KRX: 상장예정 공모주 리스트
            var krxList = await GetKrxIpoScheduleListAsync();

            foreach (var item in krxList)
            {
                string isuCd = item.Value<string>("ISU_CD") ?? "";
                string isuNm = item.Value<string>("ISU_NM") ?? "";
                string market = item.Value<string>("MKT_NM") ?? "";
                string listDate = item.Value<string>("LIST_DDT") ?? "";
                string underwriter = item.Value<string>("LEAD_MNG") ?? "";

                // 3️⃣ KRX: 종목별 상세 청약정보
                var detailList = await GetKrxIpoSubscriptionDetailAsync(isuCd);

                string subStart = "", subEnd = "", refund = "";
                if (detailList.Count > 0)
                {
                    var d = detailList[0];
                    subStart = d["SUBS_START_DT"]?.ToString() ?? "";
                    subEnd = d["SUBS_END_DT"]?.ToString() ?? "";
                    refund = d["REFUND_DT"]?.ToString() ?? "";
                }

                // 4️⃣ DART 매칭
                JObject dart = null;
                foreach (var d in dartList)
                {
                    if (isuNm.Replace("(주)", "").Trim().Contains(d.Value<string>("corp_name") ?? ""))
                    {
                        dart = (JObject)d;
                        break;
                    }
                }

                // 5️⃣ 결과 구성
                var ipo = new clsIPOData
                {
                    Company = isuNm,
                    CodeId = isuCd,
                    Exchange = market,
                    ListingDate = listDate,
                    Underwriter = underwriter,
                    SubscriptionDate = $"{subStart}~{subEnd}",
                    RefundDate = refund,
                    ConfirmedPrice = dart?["isu_pric"]?.ToString() ?? "",
                    DesiredPrice = dart?["hope_prc_rang"]?.ToString() ?? "",
                    TotalNumber = dart?["isu_am"]?.ToString() ?? "",
                    CompanyInfo = dart?["hm_url"]?.ToString() ?? "",
                    InfoUrl = dart != null ? $"https://dart.fss.or.kr/dsaf001/main.do?rcpNo={dart.Value<string>("rcept_no")}" : ""
                };

                result.Add(ipo);
            }

            Console.WriteLine($"✅ 총 {result.Count}건 IPO 청약정보 수집 완료");
            return result;
        }

        // ───────────────────────────────────────
        // ① OpenDART - 증권신고서(지분증권)
        // ───────────────────────────────────────
        private async Task<JArray> GetDartIPOListAsync(string start, string end)
        {
            var total = new JArray();

            for (int page = 1; page <= 30; page++)
            {
                string url = $"https://opendart.fss.or.kr/api/list.json?crtfc_key={_dartKey}&bgn_de={start}&end_de={end}&page_no={page}&corp_cls=E&page_count=100";
                string json = await _client.GetStringAsync(url);
                JObject obj = JObject.Parse(json);
                if (obj["status"]?.ToString() != "000") break;

                var list = obj["list"] as JArray;
                if (list == null || list.Count == 0) break;

                foreach (var item in list)
                {
                    string name = item["report_nm"]?.ToString() ?? "";
                    if (name.Contains("증권신고서") && name.Contains("지분증권"))
                        total.Add(item);
                }

                if (list.Count < 100) break;
                await Task.Delay(200);
            }

            Console.WriteLine($"[DART] {total.Count}건의 증권신고서(지분증권) 발견");
            return total;
        }

        // ───────────────────────────────────────
        // ② KRX - 상장예정 IPO 목록 (MDCSTAT06002)
        // ───────────────────────────────────────
        private async Task<JArray> GetKrxIpoScheduleListAsync()
        {
            try
            {
                const string url = "https://data.krx.co.kr/comm/bldAttendant/getJsonData.cmd";

                var payload = new Dictionary<string, string>
        {
            { "bld", "dbms/MDC/STAT/standard/MDCSTAT06002" },
            { "mktId", "ALL" },          // 코스피+코스닥 전체
            { "pubType", "ALL" },        // 공모구분 전체 (일반/스팩/기술특례 등)
            { "share", "1" },
            { "csvxls_isNo", "false" },
            { "locale", "ko_KR" }
        };

                var content = new FormUrlEncodedContent(payload);

                var req = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };

                // Referer는 반드시 06002 페이지로 맞춰야 함
                req.Headers.Add("Origin", "https://data.krx.co.kr");
                req.Headers.Add("Referer", "https://data.krx.co.kr/contents/MDC/STAT/standard/MDCSTAT06002.jsp");
                req.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                req.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");

                var res = await _client.SendAsync(req);
                var json = await res.Content.ReadAsStringAsync();

                if (json.StartsWith("<html"))
                {
                    Console.WriteLine("[KRX] HTML 에러페이지 수신됨");
                    return new JArray();
                }

                var obj = JObject.Parse(json);

                // KRX는 OutBlock_1 또는 output으로 줄 때가 있음 → 둘 다 체크
                var arr = obj["OutBlock_1"] as JArray ?? obj["output"] as JArray;
                Console.WriteLine($"[KRX IPO LIST] {(arr != null ? arr.Count : 0)}건 조회됨");
                return arr ?? new JArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[KRX] 상장예정 조회 오류: {ex.Message}");
                return new JArray();
            }
        }


        // ───────────────────────────────────────
        // ③ KRX - 종목별 청약상세 (MDCSTAT06001)
        // ───────────────────────────────────────
        private async Task<JArray> GetKrxIpoSubscriptionDetailAsync(string isuCd)
        {
            return await PostKrxAsync("dbms/MDC/STAT/standard/MDCSTAT06001", new Dictionary<string, string>
            {
                { "isuCd", isuCd },
                { "locale", "ko_KR" }
            });
        }

        // ───────────────────────────────────────
        // ④ KRX 요청 공통 함수
        // ───────────────────────────────────────
        private async Task<JArray> PostKrxAsync(string bld, Dictionary<string, string> parameters)
        {
            try
            {
                string url = "https://data.krx.co.kr/comm/bldAttendant/getJsonData.cmd";
                parameters["bld"] = bld;

                var content = new FormUrlEncodedContent(parameters);
                var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };

                var parts = bld.Split('/');
                var last = parts[parts.Length - 1];
                req.Headers.Add("Origin", "https://data.krx.co.kr");
                req.Headers.Add("Referer", $"https://data.krx.co.kr/contents/MDC/STAT/standard/{last}.jsp");
                req.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                req.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");

                var res = await _client.SendAsync(req);
                string json = await res.Content.ReadAsStringAsync();

                if (json.StartsWith("<html")) return new JArray();

                JObject obj = JObject.Parse(json);
                return obj["OutBlock_1"] as JArray ?? new JArray();
            }
            catch
            {
                return new JArray();
            }
        }
    }
}
