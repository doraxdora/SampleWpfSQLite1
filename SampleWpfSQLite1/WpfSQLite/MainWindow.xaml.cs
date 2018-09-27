using System;
using System.Data.Linq;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // SampleDb.sqlite を作成（存在しなければ）
            using (var conn = new SQLiteConnection("Data Source=SampleDb.sqlite"))
            {
                // データベースに接続
                conn.Open();
                // コマンドの実行
                using (var command = conn.CreateCommand())
                {
                    // テーブルが存在しなければ作成する
                    // 種別マスタ
                    StringBuilder sb = new StringBuilder();
                    sb.Append("CREATE TABLE IF NOT EXISTS MSTKIND (");
                    sb.Append("  KIND_CD NCHAR NOT NULL");
                    sb.Append("  , KIND_NAME NVARCHAR");
                    sb.Append("  , primary key (KIND_CD)");
                    sb.Append(")");

                    command.CommandText = sb.ToString();
                    command.ExecuteNonQuery();

                    // 猫テーブル
                    sb.Clear();
                    sb.Append("CREATE TABLE IF NOT EXISTS TBLCAT (");
                    sb.Append("  NO INT NOT NULL");
                    sb.Append("  , NAME NVARCHAR NOT NULL");
                    sb.Append("  , SEX NVARCHAR NOT NULL");
                    sb.Append("  , AGE INT DEFAULT 0 NOT NULL");
                    sb.Append("  , KIND_CD NCHAR DEFAULT 0 NOT NULL");
                    sb.Append("  , FAVORITE NVARCHAR");
                    sb.Append("  , primary key (NO)");
                    sb.Append(")");

                    command.CommandText = sb.ToString();
                    command.ExecuteNonQuery();

                    // 種別マスタを取得してコンボボックスに設定する
                    using (DataContext con = new DataContext(conn))
                    {
                        // データを取得
                        Table<Kind> mstKind = con.GetTable<Kind>();
                        IQueryable<Kind> result = from x in mstKind orderby x.KindCd select x;

                        // 最初の要素は「指定なし」とする
                        Kind empty = new Kind();
                        empty.KindCd = "";
                        empty.KindName = "指定なし";
                        var list = result.ToList();
                        list.Insert(0, empty);

                        // コンボボックスに設定
                        this.search_kind.ItemsSource = list;
                        this.search_kind.DisplayMemberPath = "KindName";
                    }

                }
                // 切断
                conn.Close();
            }
        }

        /// <summary>
        /// 検索ボタンクリックイベント.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void search_button_Click(object sender, RoutedEventArgs e)
        {
            using (var conn = new SQLiteConnection("Data Source=SampleDb.sqlite"))
            {
                conn.Open();

                // 猫データ一覧を取得して DataGrid に設定
                using (DataContext con = new DataContext(conn))
                {
                    String searchName = this.search_name.Text;
                    String searchKind = (this.search_kind.SelectedValue as Kind).KindCd;

                    // データを取得
                    Table<Cat> tblCat = con.GetTable<Cat>();

                    // サンプルなので適当に組み立てる
                    IQueryable<Cat> result;
                    if (searchKind == "")
                    {
                        // 名前は前方一致のため常に条件していしても問題なし
                        result = from x in tblCat
                                 where x.Name.StartsWith(searchName)
                                 orderby x.No
                                 select x;
                    }
                    else
                    {
                        result = from x in tblCat
                                 where x.Name.StartsWith(searchName) & x.Kind == searchKind
                                 orderby x.No
                                 select x;

                    }
                    this.dataGrid.ItemsSource = result.ToList();
                }

                conn.Close();
            }

        }
    }
}