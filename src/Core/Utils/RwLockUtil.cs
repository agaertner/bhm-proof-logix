using System;
using System.Threading;

namespace Nekres.ProofLogix.Core.Utils {
    public static class RwLockUtil {
        public static void AcquireWriteLock(ReaderWriterLockSlim rwLock, ref bool lockAcquired) {
            try {
                if (lockAcquired) {
                    return;
                }
                rwLock.EnterWriteLock();
                lockAcquired = true;
            } catch (Exception ex) {
                ProofLogix.Logger.Debug(ex, ex.Message);
            }
        }

        public static void ReleaseWriteLock(ReaderWriterLockSlim rwLock, ref bool lockAcquired, ManualResetEvent lockReleased) {
            try {
                if (!lockAcquired) {
                    return;
                }
                rwLock.ExitWriteLock();
                lockAcquired = false;
            } catch (Exception ex) {
                ProofLogix.Logger.Debug(ex, ex.Message);
            } finally {
                lockReleased.Set();
            }
        }
    }
}
