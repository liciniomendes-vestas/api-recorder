using Microsoft.Data.Sqlite;

namespace Presentation.Api;

public class Storage : IDisposable
{
	private readonly SqliteConnection _dbConnection = new($"Data Source={DateTime.UtcNow:yyyyMMddHHmmssttt}.sqlite;Cache=Shared");
    
    public Task AddAsync(Request request)
    {
	    var addDbCommand = _dbConnection.CreateCommand();
	    addDbCommand.CommandText = @"
INSERT INTO
	Requests(Method, Path, Headers, Body, InsertedAt)
VALUES 
	(@method, @path, @headers, @body, @insertedAt)
";
	    addDbCommand.Parameters.AddWithValue("@method", request.Method);
	    addDbCommand.Parameters.AddWithValue("@path", request.Path);
	    addDbCommand.Parameters.AddWithValue("@headers", request.Headers);
	    addDbCommand.Parameters.AddWithValue("body", request.Body);
	    addDbCommand.Parameters.AddWithValue("@insertedAt", request.Time);
	    
        return addDbCommand.ExecuteNonQueryAsync();
    }

    public async Task InitializeAsync()
    {
	    await _dbConnection.OpenAsync();
	    
        var createDbCommand = _dbConnection.CreateCommand();
        createDbCommand.CommandText = @"
CREATE TABLE Requests
(
	Id integer 
		constraint Requests_pk
			primary key autoincrement,
	Method text,
	Path text,
	Headers text,
	Body blob,
	InsertedAt text
);
";

        await createDbCommand.ExecuteNonQueryAsync();
    }

    public void Dispose()
    {
        _dbConnection.Dispose();
    }
}

public record Request(string Method, string Path, string Headers, string Body)
{
	public DateTime Time { get; } = DateTime.UtcNow;
};