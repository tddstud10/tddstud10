using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using R4nd0mApps.TddStud10.Hosts.VS.Diagnostics;

namespace R4nd0mApps.TddStud10.Hosts.VS
{
    // TODO: Move to fs
    public static class Services
    {
        public static TI GetService<TS, TI>(this IServiceProvider serviceProvider)
        {
            var s = serviceProvider.GetService(typeof(TS));
            if (s == null)
            {
                Logger.I.LogError("Unable to query for service of type {0}.", typeof(TS).FullName);
            }

            TI svc = default(TI);
            try
            {
                svc = (TI)s;
            }
            catch
            {
                Logger.I.LogError("Cannot cast service '{0}' to '{1}'.", typeof(TS).FullName, typeof(TI).FullName);
            }

            return svc;
        }

        public static T GetService<T>()
        {
            return GetService<T, T>();
        }

        public static TI GetService<TS, TI>()
        {
            return ServiceProvider.GlobalProvider.GetService<TS, TI>();
        }

    }
}
