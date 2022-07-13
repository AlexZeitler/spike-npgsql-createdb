using Npgsql;

namespace NpgsqlCreateDb.Tests;

public class DatabaseTests
{
  [Fact]
  public async Task ShouldCreateDatabase()
  {
    var postgresConnection = new NpgsqlConnection()
    {
      ConnectionString = new NpgsqlConnectionStringBuilder()
      {
        Pooling = false,
        Port = 5433,
        Host = "localhost",
        CommandTimeout = 20,
        Database = "postgres",
        Password = "123456",
        Username = "postgres"
      }.ToString()
    }; 
    
    await postgresConnection.OpenAsync();
    var dbName = $"testdb_{Guid.NewGuid().ToString().Replace("-","_")}";
    var script = $"CREATE DATABASE {dbName};";
    var command = new NpgsqlCommand(script, postgresConnection);
    await command.ExecuteNonQueryAsync();

    var testConnectionString = new NpgsqlConnectionStringBuilder()
    {
      Pooling = false,
      Port = 5433,
      Host = "localhost",
      CommandTimeout = 20,
      Database = dbName,
      Password = "123456",
      Username = "postgres"
    }.ToString();
    var testConnection = new NpgsqlConnection()
    {
      ConnectionString = testConnectionString
    };
    
    await testConnection.OpenAsync();
    var peoplesScript = @"
      create table people
      (
          id        serial,
          firstname text,
          lastname  text not null
      );

      alter table people
          owner to postgres;
      ";

    var peopleCommand = new NpgsqlCommand(peoplesScript, testConnection);
    await peopleCommand.ExecuteNonQueryAsync();

    var select = new NpgsqlCommand("SELECT * from people;", testConnection);
    var reader = await select.ExecuteReaderAsync();
    Assert.False(reader.HasRows);

    await testConnection.CloseAsync();
    await postgresConnection.CloseAsync();
  }
}
