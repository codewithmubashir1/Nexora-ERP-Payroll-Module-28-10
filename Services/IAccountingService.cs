using ERP.Identity.Data;
using ERP.Identity.Models.Accounts;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Services
{
    public record JournalLineInput(int AccountId, decimal Debit, decimal Credit, PartyType PartyType = PartyType.None, int? PartyId = null, string? Narration = null);

    public interface IAccountingService
    {
        /// <summary>Posts a balanced journal entry to the GL. Throws if debits don't equal credits — callers should build lines carefully since this is the last line of defense, not the primary validation.</summary>
        Task<JournalEntry> PostJournalEntryAsync(string sourceType, int sourceId, DateTime entryDate, string? narration, IEnumerable<JournalLineInput> lines);

        Task<string> GenerateEntryNumberAsync();
    }

    public class AccountingService : IAccountingService
    {
        private readonly ApplicationDbContext _db;

        public AccountingService(ApplicationDbContext db) => _db = db;

        public async Task<string> GenerateEntryNumberAsync()
        {
            var monthStamp = DateTime.UtcNow.ToString("yyyyMM");
            var fullPrefix = $"JE-{monthStamp}-";
            var count = await _db.JournalEntries.CountAsync(e => e.EntryNumber.StartsWith(fullPrefix));
            return $"{fullPrefix}{(count + 1):D4}";
        }

        public async Task<JournalEntry> PostJournalEntryAsync(string sourceType, int sourceId, DateTime entryDate, string? narration, IEnumerable<JournalLineInput> lines)
        {
            var lineList = lines.ToList();
            var totalDebit = lineList.Sum(l => l.Debit);
            var totalCredit = lineList.Sum(l => l.Credit);

            if (Math.Round(totalDebit, 2) != Math.Round(totalCredit, 2))
                throw new InvalidOperationException($"Journal entry is not balanced: debits {totalDebit:N2} vs credits {totalCredit:N2}.");

            var entry = new JournalEntry
            {
                EntryNumber = await GenerateEntryNumberAsync(),
                EntryDate = entryDate,
                SourceType = sourceType,
                SourceId = sourceId,
                Narration = narration,
                CreatedOn = DateTime.UtcNow
            };

            foreach (var line in lineList)
            {
                entry.Lines.Add(new JournalEntryLine
                {
                    AccountId = line.AccountId,
                    Debit = line.Debit,
                    Credit = line.Credit,
                    PartyType = line.PartyType,
                    PartyId = line.PartyId,
                    Narration = line.Narration
                });
            }

            _db.JournalEntries.Add(entry);
            await _db.SaveChangesAsync();

            return entry;
        }
    }
}
