using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DHAFacilitationAPIs.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DHAFacilitationAPIs.Infrastructure.Data;
public class StoredProcedures : IProcedureService
{
    private readonly IApplicationDbContext _context;
    public StoredProcedures(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<DynamicParameters> ExecuteAsync(string name, DynamicParameters parameters, CancellationToken cancellationToken)
    {
        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(cancellationToken);

        // wrap everything in a CommandDefinition so Dapper picks up the token
        var cmd = new CommandDefinition(
            name,
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken
        );

        await conn.ExecuteAsync(cmd);
        return parameters;
    }

    public async Task<(DynamicParameters, T?)> ExecuteWithSingleRowAsync<T>(string name,DynamicParameters parameters,CancellationToken cancellationToken)
    {
        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(cancellationToken);

        var cmd = new CommandDefinition(
            name,
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken
        );

        using var multi = await conn.QueryMultipleAsync(cmd);
        var row = await multi.ReadFirstOrDefaultAsync<T>();

        return (parameters, row);
    }

    public async Task<(DynamicParameters, List<T>)> ExecuteWithListAsync<T>(string name,DynamicParameters parameters,CancellationToken cancellationToken)
    {
        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(cancellationToken);

        var cmd = new CommandDefinition(
            name,
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken
        );

        var rows = (await conn.QueryAsync<T>(cmd)).ToList();

        return (parameters, rows);
    }

}
