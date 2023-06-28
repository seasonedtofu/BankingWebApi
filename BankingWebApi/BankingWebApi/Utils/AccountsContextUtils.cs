using BankingWebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingWebApi.Utils;

/// <summary>
/// Util class for saving accounts in a list.
/// </summary>
public static class AccountsContextUtils
{
    /// <summary>
    /// Tries to save updated account in the AccountsContext.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="account"></param>
    /// <returns>
    /// Nothing.
    /// </returns>
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
