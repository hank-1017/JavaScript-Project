Imports System.Data.SqlClient ' 引用資料庫連接所需的命名空間
Imports System.IO ' 引用讀寫檔案所需的命名空間
Imports System.Text ' 引用字串處理所需的命名空間

Module CopyDatabase ' 定義一個模組，作為程序的入口
    Sub Main()
        Dim configFilePath As String = "config.ini" ' 設定INI檔案的路徑
        Dim config As Dictionary(Of String, String) = ReadIniFile(configFilePath) ' 讀取INI檔案的內容並存入字典

        If config Is Nothing OrElse Not ValidateConfig(config) Then
            Console.WriteLine("INI 檔案參數不正確，請檢查並重試。") ' 如果INI檔案不正確，輸出錯誤訊息
            Return
        End If

        Dim sourceConnectionString As String = config("SourceConnectionString") ' 取得Source資料庫連接字串
        Dim targetConnectionString As String = config("TargetConnectionString") ' 取得Target資料庫連接字串

        If Not CheckDatabaseConnection(sourceConnectionString) Then
            Console.WriteLine("無法連接 Source 資料庫。") ' 如果無法連接Source資料庫，輸出錯誤訊息
            Return
        End If

        If Not CheckDatabaseConnection(targetConnectionString) Then
            Console.WriteLine("無法連接 Target 資料庫。") ' 如果無法連接Target資料庫，輸出錯誤訊息
            Return
        End If

        Try
            ' 讀取 Source 資料庫中的資料
            Dim sourceData As DataTable = GetDataFromSourceDB(sourceConnectionString)

            ' 將資料插入到 Target 資料庫中
            InsertDataIntoTargetDB(targetConnectionString, sourceData)

            Console.WriteLine("資料成功從 Source 資料庫拷貝到 Target 資料庫。")
        Catch ex As Exception
            Console.WriteLine("操作過程中發生錯誤: " & ex.Message) ' 捕捉例外並輸出錯誤訊息
        End Try
    End Sub

    Function ReadIniFile(ByVal filePath As String) As Dictionary(Of String, String)
        Dim result As New Dictionary(Of String, String)
        If Not File.Exists(filePath) Then
            Return Nothing ' 如果INI檔案不存在，返回Nothing
        End If

        For Each line As String In File.ReadAllLines(filePath)
            If Not String.IsNullOrWhiteSpace(line) AndAlso line.Contains("=") Then
                Dim parts() As String = line.Split(New Char() {"="c}, 2) ' 以等號分割每一行
                result(parts(0).Trim()) = parts(1).Trim() ' 將鍵值對存入字典
            End If
        Next

        Return result
    End Function

    Function ValidateConfig(ByVal config As Dictionary(Of String, String)) As Boolean
        ' 檢查配置字典是否包含必要的連接字串
        Return config.ContainsKey("SourceConnectionString") AndAlso config.ContainsKey("TargetConnectionString")
    End Function

    Function CheckDatabaseConnection(ByVal connectionString As String) As Boolean
        Try
            Using connection As New SqlConnection(connectionString)
                connection.Open() ' 嘗試開啟資料庫連接
            End Using
            Return True
        Catch
            Return False
        End Try
    End Function

    Function GetDataFromSourceDB(ByVal connectionString As String) As DataTable
        Dim query As String = "SELECT * FROM YourSourceTable" ' 定義查詢語句，選取所有資料
        Dim dataTable As New DataTable()

        Using connection As New SqlConnection(connectionString)
            Using command As New SqlCommand(query, connection)
                connection.Open()
                Using reader As SqlDataReader = command.ExecuteReader()
                    dataTable.Load(reader) ' 將查詢結果載入DataTable
                End Using
            End Using
        End Using

        Return dataTable
    End Function

    Sub InsertDataIntoTargetDB(ByVal connectionString As String, ByVal data As DataTable)
        Using connection As New SqlConnection(connectionString)
            connection.Open()

            For Each row As DataRow In data.Rows
                Dim query As String = "INSERT INTO YourTargetTable (Column1, Column2, ...) VALUES (@Column1, @Column2, ...)"
                Using command As New SqlCommand(query, connection)
                    ' 根據表結構添加參數
                    command.Parameters.AddWithValue("@Column1", row("Column1"))
                    command.Parameters.AddWithValue("@Column2", row("Column2"))
                    ' 根據需要添加其他列

                    command.ExecuteNonQuery() ' 執行插入操作
                End Using
            Next
        End Using
    End Sub
End Module