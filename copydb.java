package com.example.copydb;

import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.Produces;
import javax.ws.rs.core.MediaType;
import java.io.File;
import java.io.FileReader;
import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.ResultSet;
import java.sql.Statement;
import java.util.HashMap;
import java.util.Map;
import org.apache.commons.configuration2.Configuration;
import org.apache.commons.configuration2.builder.fluent.Configurations;
import org.apache.commons.configuration2.ex.ConfigurationException;

@Path("/copydb")
public class CopyDatabaseResource {

    @GET
    @Produces(MediaType.TEXT_PLAIN)
    public String copyDatabase() {
        Configurations configs = new Configurations();
        try {
            Configuration config = configs.ini(new File("config.ini"));
            String sourceConnectionString = config.getString("Database.SourceConnectionString");
            String targetConnectionString = config.getString("Database.TargetConnectionString");

            if (!checkDatabaseConnection(sourceConnectionString)) {
                return "無法連接 Source 資料庫。";
            }

            if (!checkDatabaseConnection(targetConnectionString)) {
                return "無法連接 Target 資料庫。";
            }

            copyData(sourceConnectionString, targetConnectionString);
            return "資料成功從 Source 資料庫拷貝到 Target 資料庫。";

        } catch (ConfigurationException e) {
            return "讀取 INI 檔案時發生錯誤: " + e.getMessage();
        } catch (Exception e) {
            return "操作過程中發生錯誤: " + e.getMessage();
        }
    }

    private boolean checkDatabaseConnection(String connectionString) {
        try (Connection connection = DriverManager.getConnection(connectionString)) {
            return connection.isValid(2);
        } catch (Exception e) {
            return false;
        }
    }

    private void copyData(String sourceConnectionString, String targetConnectionString) throws Exception {
        try (Connection sourceConnection = DriverManager.getConnection(sourceConnectionString);
             Connection targetConnection = DriverManager.getConnection(targetConnectionString);
             Statement sourceStatement = sourceConnection.createStatement();
             Statement targetStatement = targetConnection.createStatement()) {

            ResultSet resultSet = sourceStatement.executeQuery("SELECT * FROM YourSourceTable");
            while (resultSet.next()) {
                String column1 = resultSet.getString("Column1");
                String column2 = resultSet.getString("Column2");
                // 根據表結構添加其他列

                String query = String.format("INSERT INTO YourTargetTable (Column1, Column2, ...) VALUES ('%s', '%s', ...)", column1, column2);
                targetStatement.executeUpdate(query);
            }
        }
    }
}