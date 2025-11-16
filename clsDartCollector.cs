using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;
using HtmlAgilityPack;
using System.IO;
using System.Net;
using ExcelDataReader;
using System.Text;
using System.Data;

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

            var krxList = GetKrxIpoGrid("2025-05-16", "2025-11-16");
            return result;

            // 1️⃣ DART: 최근 증권신고서(지분증권)
            var dartList = await GetDartIPOListAsync(startDate, endDate);

            clsDBinfo m_DBInfo = new clsDBinfo();
            m_DBInfo.InitDBInfo();
            var companyList = m_DBInfo.GET_COMPANY();

            var dartReportList = ConvertDartList(dartList);

            var newList = dartReportList
                .Where(r => !companyList.Contains(r.Company) &&
                            !r.Company.Contains("인수목적"))
                .ToList();
            //번호 합치기
            //데이터 없는것만
            //https://dart.fss.or.kr/dsaf001/main.do?rcpNo=20251031000457
            //https://dart.fss.or.kr/dsaf001/main.do?rcpNo={rcept_no}
            //		ReportName	"[발행조건확정]증권신고서(지분증권)"	승인종목 20251105000307 더핑크퐁컴퍼니
            // 
            //https://kind.krx.co.kr/listinvstg/pubofrprogcom.do?method=searchPubofrProgComMain

            // 2️⃣ KRX: 상장예정 공모주 리스트
            //var krxList = await GetIpoListAsync();

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

        public List<DartReport> ConvertDartList(JArray array)
        {
            var result = new List<DartReport>();

            foreach (var item in array)
            {
                var report = new DartReport
                {
                    Company = item["corp_name"]?.ToString() ?? "",
                    ReportName = item["report_nm"]?.ToString() ?? "",
                    RceptNo = item["rcept_no"]?.ToString() ?? ""
                };

                result.Add(report);
            }

            return result;
        }
        // ───────────────────────────────────────
        // ② KRX - 상장예정 IPO 목록 (MDCSTAT06002)
        // ───────────────────────────────────────
        /// <summary>
        /// KIND 공모기업현황에서 IPO 리스트를 가져온다.
        /// $"https://kind.krx.co.kr/listinvstg/pubofrprogcom.do?method=searchPubofrProgComMain" +
        /// $"&fromDate={startDate.Replace("-", "")}" +
        /// $"&toDate={endDate.Replace("-", "")}";
        /// </summary>
        public async Task<List<KrxIpoItem>> GetKrxIpoGrid(string startDate, string endDate)
        {
            // xls 인코딩 지원 등록
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var handler = new HttpClientHandler();
            handler.UseCookies = true;
            handler.CookieContainer = new CookieContainer();
            handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            HttpClient client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://kind.krx.co.kr");

            client.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0 Safari/537.36");

            // KIND 엑셀 다운로드 요청
            var form = new MultipartFormDataContent();
            form.Add(new StringContent("searchPubofrProgComSub"), "method");
            form.Add(new StringContent("pubofrprogcom_down"), "forward");
            form.Add(new StringContent("3000"), "currentPageSize");
            form.Add(new StringContent("1"), "pageIndex");
            form.Add(new StringContent(""), "marketType");
            form.Add(new StringContent(""), "searchCorpName");
            form.Add(new StringContent(startDate.Replace("-", "")), "fromDate");
            form.Add(new StringContent(endDate.Replace("-", "")), "toDate");

            HttpResponseMessage response = await client.PostAsync("/listinvstg/pubofrprogcom.do", form);
            response.EnsureSuccessStatusCode();

            byte[] excelBytes = await response.Content.ReadAsByteArrayAsync();
            List<KrxIpoItem> result = new List<KrxIpoItem>();
            string path = @"C:\Temp\공모기업현황.xls";
            string dir = Path.GetDirectoryName(path);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllBytes(path, excelBytes);

            using (var stream = new MemoryStream(excelBytes))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    bool isHeader = true;

                    while (reader.Read())
                    {
                        if (isHeader) { isHeader = false; continue; }

                        if (reader.FieldCount < 9) continue;
                        if (reader.GetValue(0) == null) continue;

                        KrxIpoItem item = new KrxIpoItem();
                        item.Company = reader.GetValue(0)?.ToString()?.Trim();
                        item.SubmitDate = reader.GetValue(1)?.ToString()?.Trim();
                        item.ForecastDate = reader.GetValue(2)?.ToString()?.Trim();
                        item.SubscriptionDate = reader.GetValue(3)?.ToString()?.Trim();
                        item.PaymentDate = reader.GetValue(4)?.ToString()?.Trim();
                        item.ConfirmedPrice = reader.GetValue(5)?.ToString()?.Trim();
                        item.OfferingAmount = reader.GetValue(6)?.ToString()?.Trim();
                        item.ListingDate = reader.GetValue(7)?.ToString()?.Trim();
                        item.Underwriter = reader.GetValue(8)?.ToString()?.Trim();

                        result.Add(item);
                    }
                }
            }

            client.Dispose();
            return result;
        }
    }
    public class DartReport
    {
        public string Company { get; set; }
        public string ReportName { get; set; }
        public string RceptNo { get; set; }
    }
    public class KrxIpoItem
    {
        public string Company { get; set; }
        public string SubmitDate { get; set; }
        public string ForecastDate { get; set; }
        public string SubscriptionDate { get; set; }
        public string PaymentDate { get; set; }
        public string ConfirmedPrice { get; set; }
        public string OfferingAmount { get; set; }
        public string ListingDate { get; set; }
        public string Underwriter { get; set; }
    }
}
