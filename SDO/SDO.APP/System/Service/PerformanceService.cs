using SDO.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace SDO.Services
{
    public class PerformanceService : Service, IPerformanceService
    {
        private readonly PerformanceCounter cpuCounter;
        private readonly PerformanceCounter ramCounter;
        private IList<PerformanceCounter> networkCounters;
        private readonly PerformanceCounterCategory netCounterCategory = new PerformanceCounterCategory("Network Interface");

        public PerformanceService()
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
            networkCounters = new List<PerformanceCounter>();
            foreach (string networkInstanceName in netCounterCategory.GetInstanceNames())
            {
                networkCounters.Add(new PerformanceCounter("Network Interface", "Bytes Total/sec", networkInstanceName));
            }

            // CPU NETWORK 取第一次value皆為零，需要先做一次，然後等一秒，故於站台啟動時即先取一次。
            cpuCounter.NextValue();
            foreach (PerformanceCounter performanceCounter in networkCounters)
            {
                if (!netCounterCategory.InstanceExists(performanceCounter.InstanceName))
                    continue;
                performanceCounter.NextValue();
            }
        }

        public IList<PerformanceModel> GetPerformance()
        {
            IList<PerformanceModel> result = new List<PerformanceModel>();
            result.Add(new PerformanceModel() { CategoryName = "CPU", Value = cpuCounter.NextValue(), Unit = "(%)" });
            result.Add(new PerformanceModel() { CategoryName = "RAM", Value = ramCounter.NextValue(), Unit = "(%)" });

            //取得硬碟剩餘空間
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                if (!drive.IsReady)
                    continue;

                result.Add(new PerformanceModel()
                {
                    CategoryName = "HDD " + drive.Name,
                    Value = ((float)(drive.TotalSize - drive.TotalFreeSpace) / drive.TotalSize) * 100,
                    Unit = "(%)"
                });
            }

            float netWorkValue;
            IList<PerformanceCounter> needDel = networkCounters.Where(c => !netCounterCategory.InstanceExists(c.InstanceName)).ToList();
            networkCounters = networkCounters.Except(needDel).ToList();
            netWorkValue = networkCounters.Max(c => c.NextValue());

            result.Add(new PerformanceModel() { CategoryName = "Network", Value = netWorkValue / 1024, Unit = "(KB/s)" });

            return result;
        }
    }
}
