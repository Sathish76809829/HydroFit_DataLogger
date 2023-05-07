using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Petronash.Configurations;
using Petronash.Models;
using RMS.Service.Abstractions;
using RMS.Service.Abstractions.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Petronash
{
    /// <summary>
    /// Petronash Test detail service which will update start and stop entry for database
    /// </summary>
    public class TestDetailsService
    {
        static readonly ISignalComparer</*int*/string> stateComparer = default(PumpStateComparer);

        private readonly DbSet<TestDetails> testDetails;

        private readonly IDbContext _context;

        private List<PumpState> states;

        private readonly PetronashSettings settings;

        public TestDetailsService(IDbContext context, IOptions<PetronashSettings> options)
        {
            _context = context;
            testDetails = context.Set<TestDetails>();
            settings = options.Value;
        }

        public async ValueTask UpdateDetails(ParsedSet items)
        {
            if (states == null)
            {
                states = settings.Pumps.ConvertAll(p => new PumpState(p));
                foreach (var item in states)
                {
                    item.Details = await GetLastDetails(item);
                }
            }

            int stateCount = states.Count;
            for (int i = 0; i < stateCount; i++)
            {
                PumpState state = states[i];
                if (items.TryGetValue(state.StartSignal, stateComparer, out var data)
                    && data.DataValue.Equals(PumpStateBoxed.High)
                    && state.Status == PumpStatus.Stoped)
                {
                    state.Details = new TestDetails
                    {
                        DeviceId = data.DeviceId,
                        SignalId = data.SignalId,
                        StartTime = DateTime.Parse(data.TimeReceived)
                    };
                    state.Status = PumpStatus.Started;
                    await testDetails.AddAsync(state.Details);
                    await _context.SaveChangesAsync();
                    continue;
                }
                if (items.TryGetValue(state.StopSignal, stateComparer, out data)
                    && data.DataValue.Equals(PumpStateBoxed.High)
                    && state.Status == PumpStatus.Started)
                {
                    state.Details.StopTime = DateTime.Parse(data.TimeReceived);
                    state.Details.UpdateRunTime();
                    state.Status = PumpStatus.Stoped;
                    testDetails.Update(state.Details);
                    await _context.SaveChangesAsync();
                    continue;
                }
            }
        }

        public async Task OnPumpStateChanged(PumpStatusChange statusChange)
        {
            if (statusChange.Value == PumpStatus.None)
                return;
            TestDetails details = await testDetails
                   .Where(d => d.SignalId == statusChange.SignalId)
                   .OrderByDescending(d => d.TestId)
                   .FirstOrDefaultAsync();
            if (statusChange.Value == PumpStatus.Started
                && (details == null || details.StopTime.HasValue))
            {
                details = new TestDetails()
                {
                    SignalId = statusChange.SignalId,
                    DeviceId = statusChange.DeviceId,
                    StartTime = statusChange.Date
                };
                await testDetails.AddAsync(details);
            }
            else if (statusChange.Value == PumpStatus.Stoped && details == null)
            {
                details = new TestDetails()
                {
                    SignalId = statusChange.SignalId,
                    DeviceId = statusChange.DeviceId,
                    StartTime = statusChange.Date,
                    StopTime = statusChange.Date,
                    RunTime = 0
                };
                await testDetails.AddAsync(details);
            }
            else if (statusChange.Value == PumpStatus.Stoped)
            {
                details = await testDetails
                   .Where(d => d.SignalId == statusChange.SignalId)
                   .LastOrDefaultAsync();
                details.StopTime = statusChange.Date;
                details.RunTime = (details.StartTime - statusChange.Date).Ticks;
                testDetails.Update(details);
            }
            var rows = await _context.SaveChangesAsync();
        }

        async Task<TestDetails> GetLastDetails(PumpState status)
        {
            return await testDetails
                    .Where(d => d.SignalId == status.StartSignal)
                    .OrderByDescending(d => d.TestId)
                    .FirstOrDefaultAsync();
        }
    }
}
