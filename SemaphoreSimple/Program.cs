using System;
using System.Threading;

namespace SemaphoreSimple
{
    /// <summary>
    /// This simple class demonstrates how we can create our own simple Semaphore logic by uisng .Net Framework's Monitor.Wait() and Monitor.Pulse() methods.
    /// This class is just for demo purpose. It has no error handling and is not ready for production use.
    /// </summary>
    public class SemaphoreSimple
    {
        // Create a private locker object
        private object _Locker = new object();

        // This private variable indicates how many more threads can be permitted to enter critical region at any given point of time.
        // Whenever any thread enters critical region, it decrements this count by 1 and whenever any thread leaves critical region,
        // it increments this count by 1
        private int _PermittedThreads;

        // This variable holds the value of how many maximum concurrent threads can be allowed inside critical region. This variable
        // is needed only for printing messages on the Console.
        private int _MaxAllowedConcurrentThreads;

        /// <summary>
        /// Read only property which indicates how many more threads can be permitted to enter critical region at any given point of time
        /// </summary>
        public int PermittedThreads 
        {
            get
            {
                lock (_Locker)
                {
                    return _PermittedThreads;
                }
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        private SemaphoreSimple()
        {
            // Make the default constructor private so no one can use it to initialize this class
        }

        /// <summary>
        /// Public constructor with one parameter
        /// </summary>
        /// <param name="maxAllowedConcurrentThreads">Maximum number of concurrent threads that can be allowed to enter the critical region at any given point of time</param>
        public SemaphoreSimple(int maxAllowedConcurrentThreads)
        {
            if (maxAllowedConcurrentThreads < 1)
                throw new ApplicationException("Max allowed threads can't be less than 1"); // If we allow this number to be less than one, then no thread will be ever able to enter critical region

            // Store value of maxAllowedThreads in local varaibles
            _PermittedThreads = maxAllowedConcurrentThreads;
            _MaxAllowedConcurrentThreads = maxAllowedConcurrentThreads;
        }

        /// <summary>
        /// This method should be called while entering the critical region. If the count of permitted threads is greater than zero, then
        /// it will reduce it by 1 and complete execution of this method and enter the critical region. However, if the count of permitted 
        /// threads is less than 1, then the thread executing this method will go in wait state by calling Monitor.Wait() method and it will 
        /// resume only when it wakes up as a result of some other thread callign Monitor.Pulse() method. Once the thread excuting this method 
        /// resumes, it will decrement the count of permitted threads by 1 and complete the execution of this method and enter the critical region.   
        /// </summary>
        public void WaitOne()
        {
            lock (_Locker)
            {
                if (_PermittedThreads > 0)
                    _PermittedThreads--;
                else
                {
                    Monitor.Wait(_Locker);
                    _PermittedThreads--;
                }
                // This is an informational message only. Should be commented out if not needed
                Console.WriteLine($"--->> {Thread.CurrentThread.Name} entered the critical region. Total Threads in critical region = {_MaxAllowedConcurrentThreads - _PermittedThreads}");
            }
        }

        /// <summary>
        /// This method should be called while leaving the critical region. This method increases the count of permitted threads by 1 and calls
        /// Monitor.Pulse() method so that one of the waiting threads, if any, can enter the critical region.
        /// </summary>
        public void Release()
        {
            lock (_Locker)
            {
                _PermittedThreads++;

                if (_PermittedThreads > 0)
                {
                    Monitor.Pulse(_Locker);
                }
                // This is an informational message only. Should be commented out if not needed
                Console.WriteLine($"<<--- {Thread.CurrentThread.Name} left the critical region.... Total Threads in critical region = {_MaxAllowedConcurrentThreads - _PermittedThreads}");
            }
        }
    }

    /// <summary>
    /// Main entry point of the application
    /// </summary>
    class Program
    {
        static int  _WorkerThreadCount = 9, 
                    _MaxAllowedConcurrentThreads = 3, 
                    _WorkerThreadSleepTime = 2000; // 2 seconds
        
        static Thread[] threads = new Thread[_WorkerThreadCount];
        static SemaphoreSimple semSimple = new SemaphoreSimple(_MaxAllowedConcurrentThreads);

        static void Main(string[] args)
        {
            Console.WriteLine("*******************************************************");
            Console.WriteLine("Semaphore demo by uisng simple custom Semaphore class");
            Console.WriteLine("*******************************************************");
            Console.WriteLine("");
            Console.WriteLine($"Total number of worker threads = {_WorkerThreadCount}");
            Console.WriteLine($"Max Permitted Concurrent Threads in Critical Region = {_MaxAllowedConcurrentThreads}");
            Console.WriteLine("");

            // Start the specified number of threads
            for (int i = 0; i < _WorkerThreadCount; i++)
            {
                threads[i] = new Thread(SemaphoreSimpleDemo);
                threads[i].Name = $"thread_{ i + 1}";
                threads[i].Start();
            }

            // Wait for all the threads to complete
            for (int i = 0; i < _WorkerThreadCount; i++)
            {
                threads[i].Join();
            }

            Console.WriteLine("All the worker threads completed processing.");

            Console.Read();
        }

        static void SemaphoreSimpleDemo()
        {
            Console.WriteLine($"***** {Thread.CurrentThread.Name} started");
            
            semSimple.WaitOne(); // Try to enter the critical region
                        
            Thread.Sleep(_WorkerThreadSleepTime); // Pretend it is working very hard
            
            semSimple.Release(); // Exit from the critical region
        }
    }
}
