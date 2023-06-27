using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BankingWebApi.Models;
using System.Net.Http.Headers;

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
                return NotFound();
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
                return BadRequest();
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
                return BadRequest();
            }

            account.Active = active;
            await TrySaveContext(account);

            return NoContent();
        }

        [HttpPut("{id}/Deposit")]
        public async Task<IActionResult> Deposit(Guid id, decimal amount)
        {
            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return BadRequest();
            }

            account.Balance += amount;
            await TrySaveContext(account);

            return NoContent();
        }

        [HttpPut("{id}/Withdraw")]
        public async Task<IActionResult> Withdraw(Guid id, decimal amount)
        {
            var account = await _context.Accounts.FindAsync(id);

            if (account == null || amount > account.Balance)
            {
                return BadRequest();
            }

            account.Balance -= amount;
            await TrySaveContext(account);

            return NoContent();
        }

        [HttpPut("{id}/Reactivate")]
        public async Task<IActionResult> ReactivateAccount(Guid id)
        {
            var account = await _context.Accounts.FindAsync(id);

            if (account == null || account.Active)
            {
                return BadRequest();
            }

            account.Active = true;
            await TrySaveContext(account);

            return NoContent();
        }

        [HttpPut("{id}/Transfer")]
        public async Task<IActionResult> Transfer(Guid id, AccountTransfer accountTransfer)
        {
            var account = await _context.Accounts.FindAsync(id);
            var accountToTransferTo = await _context.Accounts.FindAsync(accountTransfer.TransferToId);


            if (account == null || accountToTransferTo == null || accountTransfer.Amount > account.Balance)
            {
                return BadRequest();
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

        // DELETE: api/Accounts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(Guid id)
        {
            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return NotFound();
            }

            if (account.Active == false)
            {
                return BadRequest();
            }

            // TODO: what if balance > 0?

            account.Active = false;
            account.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AccountExists(Guid id)
        {
            return (_context.Accounts?.Any(e => e.Id == id)).GetValueOrDefault();
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
    }
}
