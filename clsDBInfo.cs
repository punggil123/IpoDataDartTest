using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using Npgsql;

namespace IpoDataDartTest
{
    class clsStock
    {
        public String COUNTRY; //국가코드
        public String EXCHANGE; //거래코드
        public String CODE_ID;
        public String CODE_NAME;
    }
    class clsDATA
    {
        public int YMD;
        public String CODE_ID;
        public long VOLUME;
        public double PRICE;
    }
    class clsProfitLog
    {
        public String ACCOUNT_ID;
        public String ACCOUNT_NAME;
        public int SELL;
        public int BUY;
        public int STOP;
        public double PROFIT;
        public String THEME;
    }
    class clsSELECTIONMENU
    {
        public String ACCOUNT_ID;
        public String ACCOUNT_NAME;
        public int BUYCOUNT;
        public int BUYWAITING;
        public int SELLCOUNT;
        public int SELLWAITING;
        public int STOPLOSS;
        public int STOPTYPE;
        public int BUYREINFORCE;
        public double PRICE;
        public double IMPORTANCE;
        public double PROFIT;
        public int AVRCOUNT;
        public int RSI;
        public double BUYRSI;
        public DateTime SAVE_DAY;
        public String IMPORTTANCE;

        //당일 화면에서만 사용
        public long YesterdayYesterdayVolume;
        public long YesterdayVolume;
        public long MinVolume;
        public long AverageVolume;
        public int MinGold;
        public int AverageGold;
        public int STEP;
        public int PreviousPrice;

        //로드때 RSI만 사용
        public double Avr_AU = 0; //RSI용
        public double Avr_AD = 0; //RSI용
        public double Min_RSI = 0; //RSI용
        public double Max_RSI = 0; //RSI용
        public double Avr_RSI = 0; //RSI용

        //오늘 최고 가격
        public double TodayMaxValue;

        //테마
        public String THEME;
        //업종
        public String SECTORS;
        //상태
        public int STATUS;
        //메모
        public String MEMO;

        //이동평균선
        public double MA20 = 0;
        public double MA60 = 0;
        public double MA120 = 0;
        public double MA200 = 0;
    }
    class clsLOG
    {
        public String ACCOUNT_ID;
        public String ACCOUNT_NAME;
        public String DAY;
        public String SIGNAL;
        public double PRICE;
        public double ISPROFIT;
        public double PROFIT;
    }
    class clsACCOUNT
    {
        public String ACCOUNT_ID;
        public String ACCOUNT_NAME;
        public String ACCOUNT_PASSWORD;
        public DateTime SHELFLIFE;
        public int FIREWALL;
    }
    class clsPORTFOLIO
    {
        public String ACCOUNT_ID;
        public DateTime SAVE_DAY;
        public String CODE_NUM;
        public String CODE_NAME;
        public int IMPORTANCE;
        public double LONG_PROFIT;
        public double SHORT_PROFIT;
        public double Trends_All;
        public double VALUATION_GAIN;
        public int BUYING_TYPE;
        public int BUYING_WAIT;
        public int SELL_WAIT;
        public int ALLOW_STOP_LOSS;
        public int STOP_LOSS_TYPE;
        public int STOPLOSS_VALUE;
    }
    class clsDBinfo
    {
        public NpgsqlConnection SqlConn;
        public NpgsqlDataReader SqlReader;

        public clsACCOUNT IS_ACCOUNT = new clsACCOUNT();
        public List<clsACCOUNT> List_ACCOUNT;
        public List<clsPORTFOLIO> List_PORTFOLIO_LIST_LOG;
        public List<clsDATA> List_DATA;
        public List<clsSELECTIONMENU> List_SELECTIONMENU;
        public List<clsSELECTIONMENU> List_STAGSTOCKS;
        public List<clsProfitLog> List_ProfitLog;
        public List<clsLOG> List_LOG;
        public bool InitDBInfo()
        {
            try
            {
                bool bconn = SqlDBConnection();
                if (bconn == false)
                {
                    return false;
                }


                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool SqlDBConnection()
        {
            SqlConn = null;

            try
            {
                Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                String strUserID = currentConfig.AppSettings.Settings["DBUSERID"].Value;
                String strPort = currentConfig.AppSettings.Settings["DBPORT"].Value;
                String strUserPassword = currentConfig.AppSettings.Settings["DBUSERPASSWORD"].Value;
                String strHost = currentConfig.AppSettings.Settings["DBHOST"].Value;
                String strDBName = currentConfig.AppSettings.Settings["DBNAME"].Value;

                SqlConn = new NpgsqlConnection("server=" + strHost + "; Port = " + strPort + ";user id=" + strUserID + "; password=" + strUserPassword + "; database=" + strDBName);
                SqlConn.Open();

                return true;
            }
            catch (SqlException ex)
            {
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "에러", "SqlDBConnection() " + ex.ToString());
                return false;
            }
        }

        public void SqlReaderClose()
        {
            if (SqlReader != null)
            {
                if (!SqlReader.IsClosed)
                {
                    SqlReader.Close();
                }
            }
        }

        public bool IsDBConnection()
        {
            bool bCheck = false;

            if (SqlConn.State == ConnectionState.Open)
                bCheck = true;
            else
                bCheck = false;

            if (bCheck == false)
            {
                bool bInit = InitDBInfo();
                if (bInit == false)
                    return false;
            }

            return true;
        }
        public void InsertPORTFOLIO(clsPORTFOLIO pPORTFOLIO)
        {
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"INSERT	INTO    PORTFOLIO_LOG   (  ACCOUNT_ID, 
                                                                                SAVE_DAY, 
                                                                                CODE_NUM, 
                                                                                CODE_NAME, 
                                                                                IMPORTANCE, 
                                                                                LONG_PROFIT, 
                                                                                SHORT_PROFIT, 
                                                                                Trends_All,
                                                                                VALUATION_GAIN,
                                                                                BUYING_TYPE,
                                                                                BUYING_WAIT,
                                                                                SELL_WAIT,
                                                                                ALLOW_STOP_LOSS,
                                                                                STOP_LOSS_TYPE,
                                                                                STOPLOSS_VALUE
                                                                                )
                                                        VALUES   ('{0}', '{1}', '{2}', '{3}', {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14})",
                                                        pPORTFOLIO.ACCOUNT_ID,
                                                        pPORTFOLIO.SAVE_DAY.ToString("yyyy-MM-dd HH:mm:ss"),
                                                        pPORTFOLIO.CODE_NUM,
                                                        pPORTFOLIO.CODE_NAME,
                                                        pPORTFOLIO.IMPORTANCE,
                                                        pPORTFOLIO.LONG_PROFIT,
                                                        pPORTFOLIO.SHORT_PROFIT,
                                                        pPORTFOLIO.Trends_All,
                                                        pPORTFOLIO.VALUATION_GAIN,
                                                        pPORTFOLIO.BUYING_TYPE,
                                                        pPORTFOLIO.BUYING_WAIT,
                                                        pPORTFOLIO.SELL_WAIT,
                                                        pPORTFOLIO.ALLOW_STOP_LOSS,
                                                        pPORTFOLIO.STOP_LOSS_TYPE,
                                                        pPORTFOLIO.STOPLOSS_VALUE
                                                        );
                SqlComm.ExecuteNonQuery();

                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "정보", "InsertPORTFOLIO  계정 : " + pPORTFOLIO.ACCOUNT_ID + " 종목 : " + pPORTFOLIO.CODE_NAME);

            }
            catch (SqlException ex)
            {
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "에러", "InsertNMSInfo() " + ex.ToString());
            }
            finally
            {
            }
        }
        public void GET_PORTFOLIO(string ACCOUNT_ID, string CodeName, string dtpStart, string dtpEnd)
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"SELECT account_id, save_day, code_num, code_name, importance, long_profit, short_profit, valuation_gain, buying_type, buying_wait, sell_wait, allow_stop_loss, stop_loss_type, stoploss_value, trends_all
	                                                    FROM public.portfolio_log
	                                                    where account_id ='{0}'
			                                                    AND code_name = '{1}'
			                                                    AND '{2}' <= save_day
			                                                    AND save_day <= '{3}'
	                                                    order by save_day asc",
                                                        ACCOUNT_ID,
                                                        CodeName,
                                                        dtpStart,
                                                        dtpEnd);

                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return;
                }

                List_PORTFOLIO_LIST_LOG = new List<clsPORTFOLIO>();

                while (SqlReader.Read())
                {
                    clsPORTFOLIO pPORTFOLIO = new clsPORTFOLIO();

                    pPORTFOLIO.ACCOUNT_ID = SqlReader["account_id"].ToString();
                    pPORTFOLIO.SAVE_DAY = Convert.ToDateTime(SqlReader["save_day"]);
                    pPORTFOLIO.CODE_NUM = SqlReader["code_num"].ToString();
                    pPORTFOLIO.CODE_NAME = SqlReader["code_name"].ToString();
                    pPORTFOLIO.IMPORTANCE = Convert.ToInt32(SqlReader["importance"]);
                    pPORTFOLIO.LONG_PROFIT = Convert.ToDouble(SqlReader["long_profit"]);
                    pPORTFOLIO.SHORT_PROFIT = Convert.ToDouble(SqlReader["short_profit"]);
                    pPORTFOLIO.Trends_All = Convert.ToDouble(SqlReader["trends_all"]);
                    pPORTFOLIO.VALUATION_GAIN = Convert.ToDouble(SqlReader["valuation_gain"]);
                    pPORTFOLIO.BUYING_TYPE = Convert.ToInt32(SqlReader["buying_type"]);
                    pPORTFOLIO.BUYING_WAIT = Convert.ToInt32(SqlReader["buying_wait"]);
                    pPORTFOLIO.SELL_WAIT = Convert.ToInt32(SqlReader["sell_wait"]);
                    pPORTFOLIO.ALLOW_STOP_LOSS = Convert.ToInt32(SqlReader["allow_stop_loss"]);
                    pPORTFOLIO.STOP_LOSS_TYPE = Convert.ToInt32(SqlReader["stop_loss_type"]);
                    pPORTFOLIO.STOPLOSS_VALUE = Convert.ToInt32(SqlReader["stoploss_value"]);

                    List_PORTFOLIO_LIST_LOG.Add(pPORTFOLIO);
                }
                return;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            finally
            {
                SqlReaderClose();
            }
        }

        public void GET_GoodprofitLog(string strOrderBy)
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"SELECT    A.account_name,
                                                                A.account_id,
	                                                                (SELECT COUNT(CASE WHEN B.signal = '매수' THEN 1 END) FROM Log B where A.account_name = B.account_name) as 매수, 
	                                                                (SELECT COUNT(CASE WHEN B.signal = '매도' THEN 1 END) FROM Log B where A.account_name = B.account_name) as 매도, 
	                                                                (SELECT COUNT(CASE WHEN B.signal LIKE '%손절%' THEN 1 END) FROM Log B where A.account_name = B.account_name) as 손절,
	                                                                (SELECT B.profit FROM selectionmenu B where A.account_name = B.account_name) as profit,
	                                                                (SELECT B.theme FROM selectionmenu B where A.account_name = B.account_name) as theme
                                                                FROM selectionmenu A
                                                                GROUP BY A.account_name, A.account_id--, A.profit
                                                                order by {0} desc", strOrderBy);

                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return;
                }

                List_ProfitLog = new List<clsProfitLog>();

                while (SqlReader.Read())
                {
                    clsProfitLog pProfitLog = new clsProfitLog();

                    pProfitLog.ACCOUNT_ID = SqlReader["ACCOUNT_ID"].ToString();
                    pProfitLog.ACCOUNT_NAME = SqlReader["ACCOUNT_NAME"].ToString();
                    if (!SqlReader.IsDBNull(1))
                    {
                        pProfitLog.SELL = Convert.ToInt32(SqlReader["매수"]);
                        pProfitLog.BUY = Convert.ToInt32(SqlReader["매도"]);
                        pProfitLog.STOP = Convert.ToInt32(SqlReader["손절"]);
                        pProfitLog.PROFIT = Convert.ToDouble(SqlReader["PROFIT"]);
                        pProfitLog.THEME = SqlReader["theme"].ToString();

                        if (pProfitLog.SELL > 1 && pProfitLog.BUY > 0 && pProfitLog.BUY > pProfitLog.STOP && pProfitLog.PROFIT > 30)
                            List_ProfitLog.Add(pProfitLog);
                    }
                }
                return;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public void GET_PerfectLog()
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"SELECT    A.account_name,
                                                                A.account_id,
	                                                                (SELECT COUNT(CASE WHEN B.signal = '매수' THEN 1 END) FROM Log B where A.account_name = B.account_name) as 매수, 
	                                                                (SELECT COUNT(CASE WHEN B.signal = '매도' THEN 1 END) FROM Log B where A.account_name = B.account_name) as 매도, 
	                                                                (SELECT COUNT(CASE WHEN B.signal LIKE '%손절%' THEN 1 END) FROM Log B where A.account_name = B.account_name) as 손절,
	                                                                (SELECT B.profit FROM selectionmenu B where A.account_name = B.account_name) as profit,
	                                                                (SELECT B.theme FROM selectionmenu B where A.account_name = B.account_name) as theme
                                                                FROM selectionmenu A
                                                                GROUP BY A.account_name, A.account_id
                                                                ORDER BY 손절 ASC");

                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return;
                }

                List_ProfitLog = new List<clsProfitLog>();

                while (SqlReader.Read())
                {
                    clsProfitLog pProfitLog = new clsProfitLog();

                    pProfitLog.ACCOUNT_ID = SqlReader["ACCOUNT_ID"].ToString();
                    pProfitLog.ACCOUNT_NAME = SqlReader["ACCOUNT_NAME"].ToString();
                    if (!SqlReader.IsDBNull(1))
                    {
                        pProfitLog.SELL = Convert.ToInt32(SqlReader["매수"]);
                        pProfitLog.BUY = Convert.ToInt32(SqlReader["매도"]);
                        pProfitLog.STOP = Convert.ToInt32(SqlReader["손절"]);
                        pProfitLog.PROFIT = Convert.ToDouble(SqlReader["PROFIT"]);
                        pProfitLog.THEME = SqlReader["theme"].ToString();

                        if (pProfitLog.SELL > 2 && pProfitLog.BUY > 3 && pProfitLog.STOP < 6 && pProfitLog.PROFIT > 4)
                            List_ProfitLog.Add(pProfitLog);
                    }
                }
                return;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public List<clsStock> GET_STOCK(string strEXCHANGE)
        {
            List<clsStock> ListStock = new List<clsStock>();

            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return ListStock;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"  SELECT *
	                                                    FROM PUBLIC.stock
                                                        WHERE exchange IN ('{0}')", strEXCHANGE);

                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return ListStock;
                }

                ListStock = new List<clsStock>();

                while (SqlReader.Read())
                {
                    clsStock pStock = new clsStock();

                    pStock.CODE_ID = SqlReader["code_id"].ToString();
                    pStock.CODE_NAME = SqlReader["code_name"].ToString();
                    pStock.COUNTRY = SqlReader["country"].ToString();
                    pStock.EXCHANGE = SqlReader["exchange"].ToString();
                    ListStock.Add(pStock);
                }
                return ListStock;
            }
            catch (SqlException ex)
            {
                return ListStock;
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public List<clsDATA> GET_DATA_US_LIST(string CODE_ID, string StartDay, string EndDay)
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();

            List<clsDATA> List_DATA = new List<clsDATA>();
            if (bconn == false)
                return List_DATA;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"SELECT YMD, CODE_ID, VOLUME, PRICE
	                                                    FROM PUBLIC.DATA_US
                                                        WHERE CODE_ID = '{0}' AND '{1}' <= YMD AND YMD <= '{2}'
                                                        ORDER BY YMD DESC",
                                                        CODE_ID, StartDay, EndDay);

                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return List_DATA;
                }

                while (SqlReader.Read())
                {
                    clsDATA pDATA = new clsDATA();
                    pDATA.CODE_ID = SqlReader["CODE_ID"].ToString();
                    pDATA.YMD = Convert.ToInt32(SqlReader["YMD"]);
                    pDATA.VOLUME = Convert.ToInt32(SqlReader["VOLUME"]);
                    pDATA.PRICE = Convert.ToDouble(SqlReader["PRICE"]);
                    List_DATA.Add(pDATA);
                }

                return List_DATA;
            }
            catch (SqlException ex)
            {

                return List_DATA;
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public void GET_Log(string strACCOUNTNAME)
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                List_LOG = new List<clsLOG>();

                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"SELECT A.account_id, A.account_name, A.day, A.signal, A.price, A.isprofit, A.profit
	                                                    FROM Log A
                                                        where A.account_name = '{0}'
	                                                    order by A.day desc", strACCOUNTNAME);

                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return;
                }

                while (SqlReader.Read())
                {
                    clsLOG pLOG = new clsLOG();

                    pLOG.ACCOUNT_ID = SqlReader["account_id"].ToString();
                    pLOG.ACCOUNT_NAME = SqlReader["account_name"].ToString();
                    pLOG.DAY = Convert.ToDateTime(SqlReader["day"]).ToString("yyyy-MM-dd HH:mm:ss");
                    pLOG.SIGNAL = SqlReader["signal"].ToString();
                    pLOG.PRICE = Convert.ToDouble(SqlReader["price"]);
                    pLOG.ISPROFIT = Convert.ToDouble(SqlReader["isprofit"]);
                    pLOG.PROFIT = Convert.ToDouble(SqlReader["profit"]);
                    List_LOG.Add(pLOG);
                }
                return;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public void GET_GoodISprofitLog()
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"SELECT    A.account_name,
                                                                  A.account_id,
                                                                 (SELECT COUNT(CASE WHEN B.signal = '매수' THEN 1 END) FROM Log B where A.account_name = B.account_name) as 매수, 
                                                                 (SELECT COUNT(CASE WHEN B.signal = '매도' THEN 1 END) FROM Log B where A.account_name = B.account_name) as 매도, 
                                                                 (SELECT COUNT(CASE WHEN B.signal LIKE '%손절%' THEN 1 END) FROM Log B where A.account_name = B.account_name) as 손절,
							                                     (SELECT MAX(B.Isprofit) FROM Log B WHERE B.day > '{0}' AND B.signal = '매도' AND B.Isprofit > 10 AND A.account_name = B.account_name) as profit
                                                              FROM selectionmenu A
                                                              GROUP BY A.account_name, A.account_id
                                                              order by profit DESC", DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd HH:mm:ss"));

                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return;
                }

                List_ProfitLog = new List<clsProfitLog>();

                while (SqlReader.Read())
                {
                    clsProfitLog pProfitLog = new clsProfitLog();

                    pProfitLog.ACCOUNT_NAME = SqlReader["ACCOUNT_NAME"].ToString();
                    pProfitLog.ACCOUNT_ID = SqlReader["ACCOUNT_ID"].ToString();
                    if (!SqlReader.IsDBNull(1))
                    {
                        pProfitLog.SELL = Convert.ToInt32(SqlReader["매수"]);
                        pProfitLog.BUY = Convert.ToInt32(SqlReader["매도"]);
                        pProfitLog.STOP = Convert.ToInt32(SqlReader["손절"]);
                        if (!SqlReader.IsDBNull(5))
                        {
                            pProfitLog.PROFIT = Convert.ToDouble(SqlReader["PROFIT"]);

                            if (pProfitLog.BUY > 0 && pProfitLog.PROFIT > 10)
                                List_ProfitLog.Add(pProfitLog);
                        }
                    }
                }
                return;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public void GETprofitLog(string ACCOUNT_ID)
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"SELECT    A.account_name,
                                                                A.account_id,
	                                                            (SELECT COUNT(CASE WHEN B.signal = '매수' THEN 1 END) FROM Log B where A.account_name = B.account_name) as 매수, 
	                                                            (SELECT COUNT(CASE WHEN B.signal = '매도' THEN 1 END) FROM Log B where A.account_name = B.account_name) as 매도, 
	                                                            (SELECT COUNT(CASE WHEN B.signal LIKE '%손절%' THEN 1 END) FROM Log B where A.account_name = B.account_name) as 손절,
	                                                            (SELECT B.profit FROM selectionmenu B where A.account_name = B.account_name) as profit
                                                            FROM selectionmenu A
                                                            WHERE account_id = '{0}'", ACCOUNT_ID);

                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return;
                }

                List_ProfitLog = new List<clsProfitLog>();

                while (SqlReader.Read())
                {
                    clsProfitLog pProfitLog = new clsProfitLog();

                    pProfitLog.ACCOUNT_NAME = SqlReader["ACCOUNT_NAME"].ToString();
                    pProfitLog.ACCOUNT_ID = SqlReader["ACCOUNT_ID"].ToString();
                    if (!SqlReader.IsDBNull(1))
                    {
                        pProfitLog.SELL = Convert.ToInt32(SqlReader["매수"]);
                        pProfitLog.BUY = Convert.ToInt32(SqlReader["매도"]);
                        pProfitLog.STOP = Convert.ToInt32(SqlReader["손절"]);
                        pProfitLog.PROFIT = Convert.ToDouble(SqlReader["PROFIT"]);
                        List_ProfitLog.Add(pProfitLog);
                    }
                }
                return;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            finally
            {
                SqlReaderClose();
            }
        }

        public void GET_SELECTIONMENU()
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"SELECT ACCOUNT_ID, ACCOUNT_NAME, BUYCOUNT, BUYWAITING, SELLCOUNT, SELLWAITING, STOPLOSS, STOPTYPE, BUYREINFORCE, PRICE, IMPORTANCE, PROFIT, AVRCOUNT, RSI, BUYRSI, SAVE_DAY, IMPORTTANCE,THEME
	                                                    FROM PUBLIC.SELECTIONMENU
	                                                    ORDER BY ACCOUNT_NAME ASC");

                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return;
                }

                List_SELECTIONMENU = new List<clsSELECTIONMENU>();

                while (SqlReader.Read())
                {
                    clsSELECTIONMENU pSELECTIONMENU = new clsSELECTIONMENU();

                    pSELECTIONMENU.ACCOUNT_ID = SqlReader["account_id"].ToString();
                    pSELECTIONMENU.ACCOUNT_NAME = SqlReader["ACCOUNT_NAME"].ToString();
                    pSELECTIONMENU.BUYCOUNT = Convert.ToInt32(SqlReader["BUYCOUNT"]);
                    pSELECTIONMENU.BUYWAITING = Convert.ToInt32(SqlReader["BUYWAITING"]);
                    pSELECTIONMENU.SELLCOUNT = Convert.ToInt32(SqlReader["SELLCOUNt"]);
                    pSELECTIONMENU.SELLWAITING = Convert.ToInt32(SqlReader["SELLWAITING"]);
                    pSELECTIONMENU.STOPLOSS = Convert.ToInt32(SqlReader["STOPLOSS"]);
                    pSELECTIONMENU.STOPTYPE = Convert.ToInt32(SqlReader["STOPTYPE"]);
                    pSELECTIONMENU.BUYREINFORCE = Convert.ToInt32(SqlReader["BUYREINFORCE"]);
                    pSELECTIONMENU.PRICE = Convert.ToDouble(SqlReader["PRICE"]);
                    pSELECTIONMENU.IMPORTANCE = Convert.ToDouble(SqlReader["IMPORTANCE"]);
                    pSELECTIONMENU.PROFIT = Convert.ToDouble(SqlReader["PROFIT"]);
                    pSELECTIONMENU.AVRCOUNT = Convert.ToInt32(SqlReader["AVRCOUNT"]);
                    pSELECTIONMENU.RSI = Convert.ToInt32(SqlReader["RSI"]);
                    pSELECTIONMENU.BUYRSI = Convert.ToDouble(SqlReader["BUYRSI"]);
                    pSELECTIONMENU.SAVE_DAY = Convert.ToDateTime(SqlReader["SAVE_DAY"]);
                    pSELECTIONMENU.IMPORTTANCE = SqlReader["IMPORTTANCE"].ToString();
                    pSELECTIONMENU.THEME = SqlReader["THEME"].ToString();
                    List_SELECTIONMENU.Add(pSELECTIONMENU);
                }
                return;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public void GET_StagStocks_us()
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"SELECT code_id, code_name, buycount, buywaiting, sellcount, sellwaiting, stoploss, stoptype, buyreinforce, price, importance, profit, avrcount, rsi, buyrsi, save_day, status, memo, sectors, importtance, theme
	                                                    FROM public.stagstocks_us
                                                        --WHERE account_id = '251270'
	                                                    ORDER BY code_id ASC");
                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return;
                }

                List_SELECTIONMENU = new List<clsSELECTIONMENU>();

                while (SqlReader.Read())
                {
                    clsSELECTIONMENU pSELECTIONMENU = new clsSELECTIONMENU();

                    pSELECTIONMENU.ACCOUNT_ID = SqlReader["CODE_ID"].ToString();
                    pSELECTIONMENU.ACCOUNT_NAME = SqlReader["CODE_NAME"].ToString();
                    pSELECTIONMENU.SAVE_DAY = Convert.ToDateTime(SqlReader["SAVE_DAY"]);

                    if (SqlReader["BUYCOUNT"] != DBNull.Value)
                        pSELECTIONMENU.BUYCOUNT = Convert.ToInt32(SqlReader["BUYCOUNT"]);

                    if (SqlReader["BUYWAITING"] != DBNull.Value)
                        pSELECTIONMENU.BUYWAITING = Convert.ToInt32(SqlReader["BUYWAITING"]);

                    if (SqlReader["SELLCOUNt"] != DBNull.Value)
                        pSELECTIONMENU.SELLCOUNT = Convert.ToInt32(SqlReader["SELLCOUNT"]);

                    if (SqlReader["SELLWAITING"] != DBNull.Value)
                        pSELECTIONMENU.SELLWAITING = Convert.ToInt32(SqlReader["SELLWAITING"]);

                    if (SqlReader["STOPLOSS"] != DBNull.Value)
                        pSELECTIONMENU.STOPLOSS = Convert.ToInt32(SqlReader["STOPLOSS"]);

                    if (SqlReader["STOPTYPE"] != DBNull.Value)
                        pSELECTIONMENU.STOPTYPE = Convert.ToInt32(SqlReader["STOPTYPE"]);

                    if (SqlReader["BUYREINFORCE"] != DBNull.Value)
                        pSELECTIONMENU.BUYREINFORCE = Convert.ToInt32(SqlReader["BUYREINFORCE"]);

                    if (SqlReader["PRICE"] != DBNull.Value)
                        pSELECTIONMENU.PRICE = Convert.ToDouble(SqlReader["PRICE"]);

                    if (SqlReader["IMPORTANCE"] != DBNull.Value)
                        pSELECTIONMENU.IMPORTANCE = Convert.ToDouble(SqlReader["IMPORTANCE"]);

                    if (SqlReader["PROFIT"] != DBNull.Value)
                        pSELECTIONMENU.PROFIT = Convert.ToDouble(SqlReader["PROFIT"]);

                    if (SqlReader["AVRCOUNT"] != DBNull.Value)
                        pSELECTIONMENU.AVRCOUNT = Convert.ToInt32(SqlReader["AVRCOUNT"]);

                    if (SqlReader["RSI"] != DBNull.Value)
                        pSELECTIONMENU.RSI = Convert.ToInt32(SqlReader["RSI"]);

                    if (SqlReader["BUYRSI"] != DBNull.Value)
                        pSELECTIONMENU.BUYRSI = Convert.ToDouble(SqlReader["BUYRSI"]);

                    if (SqlReader["IMPORTTANCE"] != DBNull.Value)
                        pSELECTIONMENU.IMPORTTANCE = SqlReader["IMPORTTANCE"].ToString();

                    if (SqlReader["THEME"] != DBNull.Value)
                        pSELECTIONMENU.THEME = SqlReader["THEME"].ToString();

                    if (SqlReader["SECTORS"] != DBNull.Value)
                        pSELECTIONMENU.SECTORS = SqlReader["SECTORS"].ToString();

                    if (SqlReader["STATUS"] != DBNull.Value)
                        pSELECTIONMENU.STATUS = Convert.ToInt32(SqlReader["STATUS"]);

                    if (SqlReader["MEMO"] != DBNull.Value)
                        pSELECTIONMENU.MEMO = SqlReader["MEMO"].ToString();

                    List_SELECTIONMENU.Add(pSELECTIONMENU);
                }
                return;
            }
            catch (SqlException ex)
            {

                return;
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public void InsertStagStocks(string ACCOUNT_ID, string ACCOUNT_NAME)
        {
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;
                SqlComm.CommandText = String.Format(@"INSERT INTO PUBLIC.stagstocks(
	                                                    account_id, account_name, save_day)
	                                                    VALUES   ('{0}', '{1}'. '{2}')",
                                                        ACCOUNT_ID,
                                                        ACCOUNT_NAME,
                                                        DateTime.Now.AddYears(-2).ToString("yyyy-MM-dd HH:mm:ss")
                                                        );
                SqlComm.ExecuteNonQuery();
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "정보", "stagstocks 추가 : " + ACCOUNT_NAME);
            }
            catch (SqlException ex)
            {
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "에러", "stagstocks 선별 : " + ACCOUNT_NAME);
            }
            finally
            {
            }
        }

        public void DeleteStagStocks(string ACCOUNT_ID, string ACCOUNT_NAME)
        {
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;
                SqlComm.CommandText = String.Format(@"DELETE FROM public.stagstocks
	                                                    WHERE ACCOUNT_ID = '{0}'",
                                                        ACCOUNT_ID
                                                        );
                SqlComm.ExecuteNonQuery();
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "정보", "stagstocks 삭제 : " + ACCOUNT_NAME);

            }
            catch (SqlException ex)
            {
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "에러", "stagstocks 삭제 : " + ACCOUNT_NAME + ex.ToString());
            }
            finally
            {
            }
        }
        public void InsertStagStocksUs(string CODE_ID, string CODE_NAME)
        {
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;
                SqlComm.CommandText = String.Format(@"INSERT INTO PUBLIC.STAGSTOCKS_US(
	                                                    CODE_ID, CODE_NAME)
	                                                    VALUES   ('{0}', '{1}')",
                                                        CODE_ID,
                                                        CODE_NAME
                                                        );
                SqlComm.ExecuteNonQuery();

            }
            catch (SqlException ex)
            {
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "에러", "추천종목 선별 : " + CODE_ID + "   " + CODE_NAME);
            }
            finally
            {
            }
        }
        public List<string> GET_COMPANY()
        {
            List<string> companyList = new List<string>();
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return companyList;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"  SELECT company
	                                                    FROM PUBLIC.data_ipo WHERE company != '알지노믹스'");

                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return companyList;
                }

                while (SqlReader.Read())
                {
                    companyList.Add(SqlReader["company"].ToString());
                }
                return companyList;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return companyList;
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public void GET_STAGSTOCKSALL()
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"  SELECT *
	                                                    FROM PUBLIC.stagstocks");

                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return;
                }

                List_STAGSTOCKS = new List<clsSELECTIONMENU>();

                while (SqlReader.Read())
                {
                    clsSELECTIONMENU pSELECTIONMENU = new clsSELECTIONMENU();

                    pSELECTIONMENU.ACCOUNT_ID = SqlReader["account_id"].ToString();
                    pSELECTIONMENU.ACCOUNT_NAME = SqlReader["account_name"].ToString();
                    List_STAGSTOCKS.Add(pSELECTIONMENU);
                }
                return;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public void GET_STAGSTOCKS()
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"  SELECT *
	                                                    FROM PUBLIC.STAGSTOCKS A
	                                                    WHERE A.SAVE_DAY > '2023-01-01 00:00:00'
	                                                    AND A.PROFIT > 3
	                                                    AND A.RSI IS NOT NULL
	                                                    ORDER BY A.SAVE_DAY DESC");

                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return;
                }

                List_STAGSTOCKS = new List<clsSELECTIONMENU>();

                while (SqlReader.Read())
                {
                    clsSELECTIONMENU pSELECTIONMENU = new clsSELECTIONMENU();

                    pSELECTIONMENU.ACCOUNT_ID = SqlReader["account_id"].ToString();
                    pSELECTIONMENU.ACCOUNT_NAME = SqlReader["ACCOUNT_NAME"].ToString();
                    pSELECTIONMENU.BUYCOUNT = Convert.ToInt32(SqlReader["BUYCOUNT"]);
                    pSELECTIONMENU.BUYWAITING = Convert.ToInt32(SqlReader["BUYWAITING"]);
                    pSELECTIONMENU.SELLCOUNT = Convert.ToInt32(SqlReader["SELLCOUNt"]);
                    pSELECTIONMENU.SELLWAITING = Convert.ToInt32(SqlReader["SELLWAITING"]);
                    pSELECTIONMENU.STOPLOSS = Convert.ToInt32(SqlReader["STOPLOSS"]);
                    pSELECTIONMENU.STOPTYPE = Convert.ToInt32(SqlReader["STOPTYPE"]);
                    pSELECTIONMENU.BUYREINFORCE = Convert.ToInt32(SqlReader["BUYREINFORCE"]);
                    pSELECTIONMENU.PRICE = Convert.ToDouble(SqlReader["PRICE"]);
                    pSELECTIONMENU.IMPORTANCE = Convert.ToDouble(SqlReader["IMPORTANCE"]);
                    pSELECTIONMENU.PROFIT = Convert.ToDouble(SqlReader["PROFIT"]);
                    pSELECTIONMENU.AVRCOUNT = Convert.ToInt32(SqlReader["AVRCOUNT"]);
                    pSELECTIONMENU.RSI = Convert.ToInt32(SqlReader["RSI"]);
                    pSELECTIONMENU.BUYRSI = Convert.ToDouble(SqlReader["BUYRSI"]);
                    pSELECTIONMENU.SAVE_DAY = Convert.ToDateTime(SqlReader["SAVE_DAY"]);
                    pSELECTIONMENU.IMPORTTANCE = SqlReader["IMPORTTANCE"].ToString();
                    pSELECTIONMENU.THEME = SqlReader["THEME"].ToString();
                    List_STAGSTOCKS.Add(pSELECTIONMENU);
                }
                return;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public void Update_SELECTIONMENU(clsSELECTIONMENU pSELECTIONMENU)
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"UPDATE public.selectionmenu
	                                                                SET
                                                                    buycount={1}, 
                                                                    buywaiting={2}, 
                                                                    sellcount={3}, 
                                                                    sellwaiting={4}, 
                                                                    stoploss={5}, 
                                                                    stoptype={6}, 
                                                                    buyreinforce={7}, 
                                                                    price={8}, 
                                                                    importance={9}, 
                                                                    profit={10},
                                                                    avrcount={11},
                                                                    rsi='{12}',
                                                                    buyrsi={13},
                                                                    save_day='{14}',
                                                                    importtance='{15}'
	                                                                WHERE account_id = '{0}'",
                                                                    pSELECTIONMENU.ACCOUNT_ID,
                                                                    pSELECTIONMENU.BUYCOUNT,
                                                                    pSELECTIONMENU.BUYWAITING,
                                                                    pSELECTIONMENU.SELLCOUNT,
                                                                    pSELECTIONMENU.SELLWAITING,
                                                                    pSELECTIONMENU.STOPLOSS,
                                                                    pSELECTIONMENU.STOPTYPE,
                                                                    pSELECTIONMENU.BUYREINFORCE,
                                                                    pSELECTIONMENU.PRICE,
                                                                    pSELECTIONMENU.IMPORTANCE,
                                                                    pSELECTIONMENU.PROFIT,
                                                                    pSELECTIONMENU.AVRCOUNT,
                                                                    pSELECTIONMENU.RSI,
                                                                    pSELECTIONMENU.BUYRSI,
                                                                    pSELECTIONMENU.SAVE_DAY.ToString("yyyy-MM-dd HH:mm:ss"),
                                                                    pSELECTIONMENU.IMPORTTANCE);
                SqlComm.ExecuteNonQuery();

            }
            catch (SqlException ex)
            {
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "에러", "Update_SELECTIONMENU  계정 : " + pSELECTIONMENU.ACCOUNT_ID + " 종목 : " + pSELECTIONMENU.ACCOUNT_NAME + ex.ToString());
            }
            finally
            {
            }
        }
        public void Update_StagStocks(clsSELECTIONMENU pSELECTIONMENU)
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"UPDATE public.stagstocks
	                                                                SET
                                                                    buycount={1}, 
                                                                    buywaiting={2}, 
                                                                    sellcount={3}, 
                                                                    sellwaiting={4}, 
                                                                    stoploss={5}, 
                                                                    stoptype={6}, 
                                                                    buyreinforce={7}, 
                                                                    price={8}, 
                                                                    importance={9}, 
                                                                    profit={10},
                                                                    avrcount={11},
                                                                    rsi='{12}',
                                                                    buyrsi={13},
                                                                    save_day='{14}',
                                                                    importtance='{15}'
	                                                                WHERE account_id = '{0}'",
                                                                    pSELECTIONMENU.ACCOUNT_ID,
                                                                    pSELECTIONMENU.BUYCOUNT,
                                                                    pSELECTIONMENU.BUYWAITING,
                                                                    pSELECTIONMENU.SELLCOUNT,
                                                                    pSELECTIONMENU.SELLWAITING,
                                                                    pSELECTIONMENU.STOPLOSS,
                                                                    pSELECTIONMENU.STOPTYPE,
                                                                    pSELECTIONMENU.BUYREINFORCE,
                                                                    pSELECTIONMENU.PRICE,
                                                                    pSELECTIONMENU.IMPORTANCE,
                                                                    pSELECTIONMENU.PROFIT,
                                                                    pSELECTIONMENU.AVRCOUNT,
                                                                    pSELECTIONMENU.RSI,
                                                                    pSELECTIONMENU.BUYRSI,
                                                                    pSELECTIONMENU.SAVE_DAY.ToString("yyyy-MM-dd HH:mm:ss"),
                                                                    pSELECTIONMENU.IMPORTTANCE);
                SqlComm.ExecuteNonQuery();

            }
            catch (SqlException ex)
            {
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "에러", "Update_SELECTIONMENU  계정 : " + pSELECTIONMENU.ACCOUNT_ID + " 종목 : " + pSELECTIONMENU.ACCOUNT_NAME + ex.ToString());
            }
            finally
            {
            }
        }

        public void Update_SELECTIONMENU_Theme(string strACCOUNT_ID, string strSELECTIONMENU)
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"UPDATE public.selectionmenu
	                                                                SET
                                                                    theme='{1}'
	                                                                WHERE account_id = '{0}'",
                                                                    strACCOUNT_ID,
                                                                    strSELECTIONMENU);
                SqlComm.ExecuteNonQuery();

            }
            catch (SqlException ex)
            {
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "에러", "Update_SELECTIONMENU  계정 : " + strACCOUNT_ID + ex.ToString());
            }
            finally
            {
            }
        }
        public bool CheckDATA(clsDATA pDATA)
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return false;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"SELECT * FROM PUBLIC.DATA
                                                        WHERE YMD = '{0}'
	                                                    AND ACCOUNT_ID =  '{1}'",
                                                        pDATA.YMD,
                                                        pDATA.CODE_ID
                                                        );
                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return false;
                }
                return true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public bool Check_IPOData(string strCompany)
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return false;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"SELECT *
	                                                    FROM data_ipo
                                                        WHERE Company = '{0}'",
                                                        strCompany
                                                        );
                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return false;
                }
                return true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
            finally
            {
                SqlReaderClose();
            }
        }

        public bool CheckPORTFOLIO(String ACCOUNT_ID, String CODE_NUM)
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return false;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"SELECT * FROM PORTFOLIO_LOG
                                            WHERE ACCOUNT_ID = '{0}' AND CODE_NUM = '{1}' AND SAVE_DAY >= '{2}'",
                                            ACCOUNT_ID, CODE_NUM, DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));

                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return false;
                }
                return true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public bool LoginCheck(String ACCOUNT_ID, String Password)
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return false;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"SELECT ACCOUNT_ID, ACCOUNT_NAME, ACCOUNT_PASSWORD
	                                                    FROM PUBLIC.ACCOUNT
                                                        WHERE ACCOUNT_ID = '{0}' AND ACCOUNT_PASSWORD = '{1}'",
                                            ACCOUNT_ID, Password);

                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return false;
                }
                return true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public void GET_ACCOUNT()
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"SELECT ACCOUNT_ID, ACCOUNT_NAME, ACCOUNT_PASSWORD, FIREWALL, SHELFLIFE
	                                                    FROM ACCOUNT");

                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return;
                }

                List_ACCOUNT = new List<clsACCOUNT>();

                while (SqlReader.Read())
                {
                    clsACCOUNT pACCOUNT = new clsACCOUNT();
                    pACCOUNT.ACCOUNT_ID = SqlReader["account_id"].ToString();
                    pACCOUNT.ACCOUNT_NAME = SqlReader["account_name"].ToString();
                    pACCOUNT.ACCOUNT_PASSWORD = SqlReader["account_password"].ToString();
                    pACCOUNT.FIREWALL = Convert.ToInt32(SqlReader["firewall"]);
                    pACCOUNT.SHELFLIFE = Convert.ToDateTime(SqlReader["shelflife"]);
                    List_ACCOUNT.Add(pACCOUNT);
                }
                return;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public void GET_ACCOUNT(string ACCOUNT_ID)
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"SELECT ACCOUNT_ID, ACCOUNT_NAME, ACCOUNT_PASSWORD, FIREWALL, SHELFLIFE
	                                                    FROM ACCOUNT
                                                        WHERE ACCOUNT_ID ='{0}'", ACCOUNT_ID);

                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return;
                }

                List_ACCOUNT = new List<clsACCOUNT>();

                while (SqlReader.Read())
                {
                    clsACCOUNT pACCOUNT = new clsACCOUNT();
                    pACCOUNT.ACCOUNT_ID = SqlReader["account_id"].ToString();
                    pACCOUNT.ACCOUNT_NAME = SqlReader["account_name"].ToString();
                    pACCOUNT.ACCOUNT_PASSWORD = SqlReader["account_password"].ToString();
                    pACCOUNT.FIREWALL = Convert.ToInt32(SqlReader["firewall"]);
                    pACCOUNT.SHELFLIFE = Convert.ToDateTime(SqlReader["shelflife"]);
                    IS_ACCOUNT = pACCOUNT;
                }
                return;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public List<string> GET_CODELIST(string ACCOUNT_ID, string dtpStart, string dtpEnd)
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return null;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"SELECT code_name
	                                                    FROM public.portfolio_log
	                                                    where account_id ='{0}'
			                                                    AND '{1}' <= save_day
			                                                    AND save_day <= '{2}'
	                                                    GROUP BY code_name", ACCOUNT_ID, dtpStart, dtpEnd);

                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return null;
                }

                List<string> List_CODE = new List<string>();

                while (SqlReader.Read())
                {
                    List_CODE.Add(SqlReader["code_name"].ToString());
                }
                return List_CODE;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return new List<string>();
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public void DeleteaccountInfo(string ACCOUNT_ID)
        {
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"DELETE FROM ACCOUNT
	                                                    WHERE ACCOUNT_ID = '{0}'",
                                                    ACCOUNT_ID);
                SqlComm.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public void DeletDATE(string ACCOUNT_ID, string ACCOUNT_NAME)
        {
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;
                SqlComm.CommandText = String.Format(@"DELETE FROM PUBLIC.DATA
	                                                    WHERE ACCOUNT_ID = '{0}'",
                                                        ACCOUNT_ID
                                                        );
                SqlComm.ExecuteNonQuery();
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "정보", "추천종목 삭제 : " + ACCOUNT_NAME);

            }
            catch (SqlException ex)
            {
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "에러", "추천종목 삭제 : " + ACCOUNT_NAME + ex.ToString());
            }
            finally
            {
            }
        }
        public void DeletSELECTIONMENU(string ACCOUNT_ID, string ACCOUNT_NAME)
        {
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;
                SqlComm.CommandText = String.Format(@"DELETE FROM public.selectionmenu
	                                                    WHERE ACCOUNT_ID = '{0}'",
                                                        ACCOUNT_ID
                                                        );
                SqlComm.ExecuteNonQuery();
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "정보", "추천종목 삭제 : " + ACCOUNT_NAME);

            }
            catch (SqlException ex)
            {
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "에러", "추천종목 삭제 : " + ACCOUNT_NAME + ex.ToString());
            }
            finally
            {
            }
        }
        public void Insert_IPOData(clsIPOData pIPOData)
        {
            if (!IsDBConnection()) return;
            try
            {
                using (var SqlComm = new NpgsqlCommand())
                {
                    SqlComm.Connection = SqlConn;

                    SqlComm.CommandText = string.Format(@"
                                                        INSERT INTO public.data_ipo (
                                                            company, code_id, competitionrate, infourl, underwriter,
                                                            totalnumber, desiredprice, confirmedprice,
                                                            subscriptiondate, refunddate, listingdate, forecastdate, mandatoryretention,
                                                            exchange, company_info, industry, listed_shares, circulation_ratio,
                                                            institutional_competition_rate, market_cap, price_updatetime
                                                        )
                                                        VALUES (
                                                            '{0}', '{1}', '{2}', '{3}', '{4}',
                                                            '{5}', '{6}', '{7}',
                                                            '{8}', '{9}', '{10}', '{11}', '{12}',
                                                            '{13}', '{14}', '{15}', '{16}',
                                                            '{17}', '{18}', '{19}', NOW()
                                                        )",
                                                            pIPOData.Company,
                                                            pIPOData.CodeId,
                                                            pIPOData.CompetitionRate,
                                                            pIPOData.InfoUrl,
                                                            pIPOData.Underwriter,
                                                            pIPOData.TotalNumber,
                                                            pIPOData.DesiredPrice,
                                                            pIPOData.ConfirmedPrice,
                                                            pIPOData.SubscriptionDate,
                                                            pIPOData.RefundDate,
                                                            pIPOData.ListingDate,
                                                            pIPOData.ForecastDate,
                                                            pIPOData.MandatoryRetention,
                                                            pIPOData.Exchange,
                                                            pIPOData.CompanyInfo,
                                                            pIPOData.Industry,
                                                            pIPOData.ListedShares,
                                                            pIPOData.CirculationRatio,
                                                            pIPOData.InstitutionalCompetitionRate,
                                                            pIPOData.MarketCap
                                                            );

                    SqlComm.ExecuteNonQuery();
                }
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "정보", "공모주 추가 : " + pIPOData.Company);
            }
            catch (Exception ex)
            {
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "에러",
                    "공모주 추가 : " + pIPOData.Company + " | " + ex);
            }
            finally { SqlReaderClose(); }
        }
        public void InsertDATA(clsDATA pDATA, string strCountry)
        {
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                if (strCountry == "KR")
                {
                    SqlComm.CommandText = String.Format(@"INSERT INTO PUBLIC.data_kr(
	                                                    YMD, code_id, VOLUME, PRICE)
	                                                    VALUES   ({0}, '{1}', {2}, {3})",
                                                            pDATA.YMD,
                                                            pDATA.CODE_ID,
                                                            pDATA.VOLUME,
                                                            pDATA.PRICE
                                                            );
                }
                else if (strCountry == "US")
                {
                    SqlComm.CommandText = String.Format(@"INSERT INTO PUBLIC.data_us(
	                                                    YMD, code_id, VOLUME, PRICE)
	                                                    VALUES   ({0}, '{1}', {2}, {3})",
                                                            pDATA.YMD,
                                                            pDATA.CODE_ID,
                                                            pDATA.VOLUME,
                                                            pDATA.PRICE
                                                            );
                }
                SqlComm.ExecuteNonQuery();

            }
            catch (SqlException ex)
            {
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "에러", "과거데이터 업데이트 : " + pDATA.YMD.ToString() + pDATA.CODE_ID);
            }
            finally
            {
            }
        }
        public void InsertLOG(clsLOG pLOG)
        {
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"INSERT INTO PUBLIC.log(
	                                                    ACCOUNT_ID, ACCOUNT_NAME, DAY, SIGNAL, PRICE, ISPROFIT, PROFIT)
	                                                    VALUES   ('{0}', '{1}', '{2}', '{3}', {4}, {5}, {6})",
                                                        pLOG.ACCOUNT_ID,
                                                        pLOG.ACCOUNT_NAME,
                                                        pLOG.DAY,
                                                        pLOG.SIGNAL,
                                                        pLOG.PRICE,
                                                        pLOG.ISPROFIT,
                                                        pLOG.PROFIT
                                                        );
                SqlComm.ExecuteNonQuery();
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "정보", "자동매매기록 : " + pLOG.DAY.ToString() + pLOG.ACCOUNT_ID);

            }
            catch (SqlException ex)
            {
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "에러", "자동매매기록 : " + pLOG.DAY.ToString() + pLOG.ACCOUNT_ID);
            }
            finally
            {
            }
        }
        public void InitSELECTIONMENU(string ACCOUNT_ID, string ACCOUNT_NAME)
        {
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;
                SqlComm.CommandText = String.Format(@"INSERT INTO PUBLIC.SELECTIONMENU(
	                                                    ACCOUNT_ID, ACCOUNT_NAME, BUYCOUNT, BUYWAITING, SELLCOUNT, SELLWAITING, STOPLOSS, STOPTYPE, BUYREINFORCE, PRICE, IMPORTANCE, PROFIT, AVRCOUNT, RSI, BUYRSI, SAVE_DAY, IMPORTTANCE)
	                                                    VALUES   ('{0}', '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, '{13}', {14}, '{15}','{16}')",
                                                        ACCOUNT_ID,
                                                        ACCOUNT_NAME,
                                                        0, 10, 0, 10, 0, 0, 0, 0, 0, 0, 20, 0, 99, DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd HH:mm:ss"), ""
                                                        );
                SqlComm.ExecuteNonQuery();

            }
            catch (SqlException ex)
            {
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "에러", "추천종목 선별 : " + ACCOUNT_NAME);
            }
            finally
            {
            }
        }
        public void InitSELECTIONMENU(clsSELECTIONMENU pSELECTIONMENU)
        {
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;
                /*
                SqlComm.CommandText = String.Format(@"INSERT INTO PUBLIC.SELECTIONMENU(
	                                                    ACCOUNT_ID, ACCOUNT_NAME, BUYCOUNT, BUYWAITING, SELLCOUNT, SELLWAITING, STOPLOSS, STOPTYPE, BUYREINFORCE, PRICE, IMPORTANCE, PROFIT, AVRCOUNT, RSI, BUYRSI, SAVE_DAY, IMPORTTANCE)
	                                                    VALUES   ('{0}', '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, '{13}', {14}, '{15}','{16}')",
                                                        pSELECTIONMENU.ACCOUNT_ID,
                                                        pSELECTIONMENU.ACCOUNT_NAME,
                                                        pSELECTIONMENU.BUYCOUNT,
                                                        pSELECTIONMENU.BUYWAITING, 
                                                        pSELECTIONMENU.SELLCOUNT, 
                                                        pSELECTIONMENU.SELLWAITING,
                                                        pSELECTIONMENU.STOPLOSS, 
                                                        pSELECTIONMENU.STOPTYPE,
                                                        pSELECTIONMENU.BUYREINFORCE, 
                                                        0,
                                                        pSELECTIONMENU.IMPORTANCE,
                                                        0,
                                                        pSELECTIONMENU.AVRCOUNT,
                                                        pSELECTIONMENU.RSI,
                                                        pSELECTIONMENU.BUYRSI,
                                                        pSELECTIONMENU.SAVE_DAY.ToString("yyyy-MM-dd HH:mm:ss"),
                                                        pSELECTIONMENU.IMPORTTANCE
                                                        );
                */
                SqlComm.CommandText = String.Format(@"INSERT INTO PUBLIC.SELECTIONMENU(
	                                                    ACCOUNT_ID, ACCOUNT_NAME, BUYCOUNT, BUYWAITING, SELLCOUNT, SELLWAITING, STOPLOSS, STOPTYPE, BUYREINFORCE, PRICE, IMPORTANCE, PROFIT, AVRCOUNT, RSI, BUYRSI, SAVE_DAY, IMPORTTANCE)
	                                                    VALUES   ('{0}', '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, '{13}', {14}, '{15}','{16}')",
                                                        pSELECTIONMENU.ACCOUNT_ID,
                                                        pSELECTIONMENU.ACCOUNT_NAME,
                                                        pSELECTIONMENU.BUYCOUNT,
                                                        pSELECTIONMENU.BUYWAITING,
                                                        pSELECTIONMENU.SELLCOUNT,
                                                        pSELECTIONMENU.SELLWAITING,
                                                        pSELECTIONMENU.STOPLOSS,
                                                        pSELECTIONMENU.STOPTYPE,
                                                        pSELECTIONMENU.BUYREINFORCE,
                                                        pSELECTIONMENU.PRICE,
                                                        pSELECTIONMENU.IMPORTANCE,
                                                        pSELECTIONMENU.PROFIT,
                                                        pSELECTIONMENU.AVRCOUNT,
                                                        pSELECTIONMENU.RSI,
                                                        pSELECTIONMENU.BUYRSI,
                                                        pSELECTIONMENU.SAVE_DAY.ToString("yyyy-MM-dd HH:mm:ss"),
                                                        pSELECTIONMENU.IMPORTTANCE
                                                        );
                SqlComm.ExecuteNonQuery();

            }
            catch (SqlException ex)
            {
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "에러", "추천종목 선별 : " + pSELECTIONMENU.ACCOUNT_NAME);
            }
            finally
            {
            }
        }

        public bool GETDATA(clsDATA pDATA)
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return false;

            try
            {
                SqlComm.Connection = SqlConn;
                SqlComm.CommandText = String.Format(@"SELECT YMD, ACCOUNT_ID, VOLUME, PRICE
	                                                    FROM PUBLIC.DATA
                                                        WHERE ACCOUNT_ID = '{0}' AND YMD = '{1}'",
                                            pDATA.CODE_ID, pDATA.YMD);

                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return false;
                }
                return true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public bool GETDATA(clsDATA pDATA, string strCountry)
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return false;

            try
            {
                SqlComm.Connection = SqlConn;

                if (strCountry == "KR")
                {
                    SqlComm.CommandText = String.Format(@"SELECT YMD, CODE_ID, VOLUME, PRICE
	                                                    FROM PUBLIC.data_kr
                                                        WHERE CODE_ID = '{0}' AND YMD = '{1}'",
                                                pDATA.CODE_ID, pDATA.YMD);
                }
                else if (strCountry == "US")
                {
                    SqlComm.CommandText = String.Format(@"SELECT YMD, CODE_ID, VOLUME, PRICE
	                                                    FROM PUBLIC.data_us
                                                        WHERE CODE_ID = '{0}' AND YMD = '{1}'",
                                                pDATA.CODE_ID, pDATA.YMD);
                }

                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return false;
                }
                return true;
            }
            catch (SqlException ex)
            {
                return false;
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public List<clsDATA> GET_DATA_LIST(string ACCOUNT_ID, string StartDay, string EndDay)
        {
            SqlReader = null;
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();

            List_DATA = new List<clsDATA>();
            if (bconn == false)
                return List_DATA;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"SELECT YMD, ACCOUNT_ID, VOLUME, PRICE
	                                                    FROM PUBLIC.DATA
                                                        WHERE ACCOUNT_ID = '{0}' AND '{1}' <= YMD AND YMD <= '{2}'
                                                        ORDER BY YMD ASC",
                                                        ACCOUNT_ID, StartDay, EndDay);

                SqlReader = SqlComm.ExecuteReader();

                if (!SqlReader.HasRows)
                {
                    return List_DATA;
                }

                while (SqlReader.Read())
                {
                    clsDATA pDATA = new clsDATA();
                    pDATA.CODE_ID = SqlReader["ACCOUNT_ID"].ToString();
                    pDATA.YMD = Convert.ToInt32(SqlReader["YMD"]);
                    pDATA.VOLUME = Convert.ToInt32(SqlReader["VOLUME"]);
                    pDATA.PRICE = Convert.ToDouble(SqlReader["PRICE"]);
                    List_DATA.Add(pDATA);
                }

                if (List_DATA.Count < 1)
                    DeletSELECTIONMENU(ACCOUNT_ID, "GET_DATA_LIST 값이 없음");

                return List_DATA;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return List_DATA;
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public void InsertACCOUNTInfo(clsACCOUNT pACCOUNT)
        {
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"INSERT INTO PUBLIC.ACCOUNT(
	                                                    ACCOUNT_ID, ACCOUNT_NAME, ACCOUNT_PASSWORD, FIREWALL, SHELFLIFE)
	                                                    VALUES   ('{0}', '{1}', '{2}', {3}, '{4}')",
                                                        pACCOUNT.ACCOUNT_ID,
                                                        pACCOUNT.ACCOUNT_NAME,
                                                        pACCOUNT.ACCOUNT_PASSWORD,
                                                        pACCOUNT.FIREWALL.ToString(),
                                                        pACCOUNT.SHELFLIFE.ToString("yyyy-MM-dd HH:mm:ss")
                                                        );
                SqlComm.ExecuteNonQuery();

                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "정보", "추가 계좌 : " + pACCOUNT.ACCOUNT_ID);

            }
            catch (SqlException ex)
            {
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "에러", "InsertACCOUNTInfo() " + ex.ToString());
            }
            finally
            {
            }
        }
        public void Update_IPOData(clsIPOData pIPOData)
        {
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = string.Format(@"
                                                        UPDATE public.data_ipo
                                                        SET 
                                                            code_id='{1}',
                                                            competitionrate='{2}',
                                                            infourl='{3}',
                                                            underwriter='{4}',
                                                            totalnumber='{5}',
                                                            desiredprice='{6}',
                                                            confirmedprice='{7}',
                                                            subscriptiondate='{8}',
                                                            refunddate='{9}',
                                                            listingdate='{10}',
                                                            forecastdate='{11}',
                                                            mandatoryretention='{12}',
                                                            exchange='{13}',
                                                            company_info='{14}',
                                                            industry='{15}',
                                                            listed_shares='{16}',
                                                            circulation_ratio='{17}',
                                                            institutional_competition_rate='{18}',
                                                            market_cap ='{19}'
                                                        WHERE company = '{0}'",
                                                            pIPOData.Company,
                                                            pIPOData.CodeId,
                                                            pIPOData.CompetitionRate,
                                                            pIPOData.InfoUrl,
                                                            pIPOData.Underwriter,
                                                            pIPOData.TotalNumber,
                                                            pIPOData.DesiredPrice,
                                                            pIPOData.ConfirmedPrice,
                                                            pIPOData.SubscriptionDate,
                                                            pIPOData.RefundDate,
                                                            pIPOData.ListingDate,
                                                            pIPOData.ForecastDate,
                                                            pIPOData.MandatoryRetention,
                                                            pIPOData.Exchange,
                                                            pIPOData.CompanyInfo,
                                                            pIPOData.Industry,
                                                            pIPOData.ListedShares,
                                                            pIPOData.CirculationRatio,
                                                            pIPOData.InstitutionalCompetitionRate,
                                                            pIPOData.MarketCap

                                                     );
                SqlComm.ExecuteNonQuery();

                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "정보", "공모주 업데이트 : " + pIPOData.Company);

            }
            catch (Exception ex)
            {
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "에러", "공모주 업데이트 " + ex.ToString());
            }
            finally
            {
                SqlReaderClose();
            }
        }
        public void Update_IPOData_NewListing(clsIPOData pIPOData)
        {
            NpgsqlCommand SqlComm = new NpgsqlCommand();

            // DB에 대한 connection check
            // 새로 DB에 대한 접속을 연결하며 데이터를 새로 받는다.
            bool bconn = IsDBConnection();
            if (bconn == false)
                return;

            try
            {
                SqlComm.Connection = SqlConn;

                SqlComm.CommandText = String.Format(@"UPDATE data_ipo
	                                                    SET Price='{1}',
                                                            fluctuation_rate='{2}',
                                                            open_price='{3}',
                                                            open_ratio='{4}',
                                                            firstday_close='{5}'
	                                                    WHERE company = '{0}'",
                                                        pIPOData.Company,
                                                        pIPOData.Price,
                                                        pIPOData.FluctuationRate,
                                                        pIPOData.OpenPrice,
                                                        pIPOData.OpenRatio,
                                                        pIPOData.FirstDayClose
                                                        );

                SqlComm.ExecuteNonQuery();

                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "정보", "공모주 업데이트 : " + pIPOData.Company);

            }
            catch (Exception ex)
            {
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "에러", "공모주 업데이트 " + ex.ToString());
            }
            finally
            {
                SqlReaderClose();
            }
        }

        private void AddLog(String strTime, String strKind, String strLog)
        {
            try
            {
                // 파일로그 남기기
                string MyWriteFile = "C:\\Users\\heobi\\OneDrive\\바탕 화면" + @"\AI매매통합화면DB로그.dat";
                System.IO.StreamWriter FileWriter = new System.IO.StreamWriter(MyWriteFile, true, System.Text.Encoding.UTF8);

                FileWriter.WriteLine(strTime.ToString() + "," + strKind + "," + strLog);
                FileWriter.Close();
            }
            catch
            {
            }
        }
    }
}
