using BankingWebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingWebApi.Utils;

public static class AccountsContextUtils
{
    public static async Task TrySaveContext(AccountsContext context, Account account)
    {
        try
        {
            account.UpdatedDate = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw;
        }
    }
}
