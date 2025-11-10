using System;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace IpoDataDartTest
{
    public partial class MainForm : Form
    {
        // 발급받은 OpenDART API Key
        private static readonly string DartApiKey = "39e62f767e176e937e87441fabcb6c7c4e025fb5";
        private const string KrxBaseUrl = "https://data.krx.co.kr/comm/bldAttendant/getJsonData.cmd";
        private readonly HttpClient _client = new HttpClient();
        clsDBinfo m_DBInfo = new clsDBinfo();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }
        /// <summary>
        /// OpenDART API에서 IPO 공시 목록 조회
        /// </summary>
        private async Task<DataTable> GetIPOPublicScheduleDataFromDART()
        {
            //string key = Uri.EscapeDataString(apiKey.Trim());
            //string listUrl = $"https://opendart.fss.or.kr/api/list.json?crtfc_key={key}&bgn_de=20250628&end_de=20250924&page_no=1&page_count=100";

            //string listJson = await GetJsonAsync(listUrl);

            //var ipoList = ParseIpoListJson(listJson);

            // 오늘 기준 최근 3개월 예시
            string start = DateTime.Now.AddMonths(-3).ToString("yyyyMMdd");
            string end = DateTime.Now.ToString("yyyyMMdd");

            clsDartCollector dartCollector = new clsDartCollector(DartApiKey);
            var ipoList = await dartCollector.CollectIPOData(start, end);

            var dt = new DataTable();
            return dt;
        }

        /// <summary>
        /// 공통 HTTP GET 메서드
        /// </summary>
        private static async Task<string> GetJsonAsync(string url)
        {
            using (var client = new HttpClient())
            {
                // OpenDART API에서 User-Agent 없으면 error1.html 로 리다이렉트될 수 있어서 꼭 추가!
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string result = await response.Content.ReadAsStringAsync();
                return result;
            }
        }
        public bool TryParseConfirmedPrice(string input, out long value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(input)) return false;

            // 숫자만 남기기
            string clean = Regex.Replace(input, @"[^\d]", "");
            return long.TryParse(clean, out value);
        }

        private static DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;

            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);

            DateTime AfterWards = ThisMoment.Add(duration);

            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();

                ThisMoment = DateTime.Now;
            }

            return DateTime.Now;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var publicSchedule = GetIPOPublicScheduleDataFromDART();
            //공모주 리스트
            dataGridView1.DataSource = publicSchedule;
        }

    }
}
public class clsIPOData
{
    public string Company { get; set; } // 종목명
    public string CodeId { get; set; } // 종목코드
    public string InfoUrl { get; set; } // 공모정보URL
    public string Price { get; set; } // 현재가(원)
    public string Underwriter { get; set; } // 주간사
    public string CompetitionRate { get; set; } // 청약경쟁률
    public string FluctuationRate { get; set; } // 공모가대비 등락률(%)
    public string OpenPrice { get; set; } // 시초가(원)
    public string OpenRatio { get; set; } // 시초/공모(%)
    public string FirstDayClose { get; set; } // 첫날종가(원)
    public string Exchange { get; set; } // 시장구분
    public string SubscriptionDate { get; set; } // 청약일
    public string RefundDate { get; set; } // 환불일
    public string ListingDate { get; set; } // 상장일
    public string ForecastDate { get; set; } // 수요예측일
    public string MandatoryRetention { get; set; } // 의무보유확약
    public string InstitutionalCompetitionRate { get; set; } // 기관경쟁률
    public string TotalNumber { get; set; } // 총공모주식수
    public string DesiredPrice { get; set; } // 희망공모가액
    public string ConfirmedPrice { get; set; } // 확정공모가
    public string CompanyInfo { get; set; } // 회사홈페이지
    public string Industry { get; set; } // 업종
    public string ListedShares { get; set; } // 상장주식수
    public string CirculationRatio { get; set; } // 유통비율
    public string MarketCap { get; set; } // 시가총액
    public DateTime PriceUpDateTime { get; set; } // 현재가 업데이트 시간
}