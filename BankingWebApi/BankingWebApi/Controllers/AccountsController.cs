using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BankingWebApi.Models;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace BankingWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly AccountsContext _context;

        public AccountsController(AccountsContext context)
        {
            _context = context;
        }

        // GET: api/Accounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccounts()
        {
            if (_context.Accounts == null)
            {
                return NotFound();
            }
            return await _context.Accounts.ToListAsync();
        }

        // GET: api/Accounts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Account>> GetAccount(Guid id)
        {
            if (_context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return ErrorResponse("Could not find account with provided GUID.", HttpStatusCode.NotFound);
            }

            return account;
        }

        // PUT: api/Accounts/5
        [HttpPut("{id}/Name")]
        public async Task<IActionResult> ChangeName(Guid id, String name)
        {
            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return ErrorResponse("Could not find account with provided GUID.", HttpStatusCode.BadRequest);
            }

            account.Name = name;
            await TrySaveContext(account);

            return NoContent();
        }

        [HttpPut("{id}/Active")]
        public async Task<IActionResult> ChangeActive(Guid id, bool active)
        {
            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return ErrorResponse("Could not find account with provided GUID.", HttpStatusCode.BadRequest);
            }

            account.Active = active;
            await TrySaveContext(account);

            return NoContent();
        }

        [HttpPost("{id}/Deposit")]
        public async Task<IActionResult> Deposit(Guid id, decimal amount)
        {
            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return ErrorResponse("Could not find account with provided GUID.", HttpStatusCode.BadRequest);
            }

            account.Balance += amount;
            await TrySaveContext(account);

            return NoContent();
        }

        [HttpPost("{id}/Withdraw")]
        public async Task<IActionResult> Withdraw(Guid id, decimal amount)
        {
            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return ErrorResponse("Could not find account with provided GUID.", HttpStatusCode.BadRequest);
            }
            else if (amount > account.Balance)
            {
                return ErrorResponse("Amount entered is more than account balance.", HttpStatusCode.BadRequest);
            }

            account.Balance -= amount;
            await TrySaveContext(account);

            return NoContent();
        }

        [HttpPost("{id}/Transfer")]
        public async Task<IActionResult> Transfer(AccountTransfer accountTransfer)
        {
            var account = await _context.Accounts.FindAsync(accountTransfer.TransferFromId);
            var accountToTransferTo = await _context.Accounts.FindAsync(accountTransfer.TransferToId);


            if (account == null)
            {
                return ErrorResponse("Could not find transfer from account with provided GUID.", HttpStatusCode.BadRequest);
            }
            else if (accountToTransferTo == null)
            {
                return ErrorResponse("Could not find transfer to account with provided GUID.", HttpStatusCode.BadRequest);
            }
            else if (accountTransfer.Amount > account.Balance)
            {
                return ErrorResponse("Amount entered is more than account balance.", HttpStatusCode.BadRequest);
            }

            await Withdraw(account.Id, accountTransfer.Amount);
            await Deposit(accountToTransferTo.Id, accountTransfer.Amount);
            await TrySaveContext(account);

            return NoContent();
        }

        // POST: api/Accounts
        [HttpPost]
        public async Task<ActionResult<AccountCreate>> PostAccount(AccountCreate accountCreate)
        {
            var dateTime = DateTime.UtcNow;
            var account = new Account
            {
                Id = Guid.NewGuid(),
                Name = accountCreate.Name,
                Balance = accountCreate.Balance,
                Active = true,
                CreatedDate = dateTime,
                UpdatedDate = dateTime,
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
        }

        [HttpPut("{id}/Reactivate")]
        public async Task<IActionResult> ReactivateAccount(Guid id)
        {
            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return ErrorResponse("Could not find account with provided GUID.", HttpStatusCode.BadRequest);
            }
            else if (account.Active)
            {
                return ErrorResponse("Account already active.", HttpStatusCode.BadRequest);
            }

            account.Active = true;
            await TrySaveContext(account);

            return NoContent();
        }

        // DELETE: api/Accounts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(Guid id)
        {
            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return ErrorResponse("Could not find account with provided GUID.", HttpStatusCode.BadRequest);
            }
            else if (account.Active == false)
            {
                return ErrorResponse("Account already inactive.", HttpStatusCode.BadRequest);
            }
            else if (account.Balance > 0)
            {
                return ErrorResponse("Account currently has a balance greater than 0, please withdraw first.", HttpStatusCode.BadRequest);
            }



            account.Active = false;
            account.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task TrySaveContext(Account account)
        {
            try
            {
                account.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

        private ObjectResult ErrorResponse(String message, HttpStatusCode statusCode)
        {
            return new ObjectResult(message) { StatusCode = (int)statusCode };
        }
    }
}
