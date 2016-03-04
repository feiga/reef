using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.Apache.REEF.ParameterService.Multiverso.Bridge;

namespace Org.Apache.REEF.ParameterService.Multiverso.Bridge.Test
{
    class Program
    {
        static String worker_ip_port = "10.172.150.90:9999";
        static String server_ip_port = "10.172.150.90:10000";

        static int num_rows = 11;
        static int num_cols = 10;
        static int size = num_rows * num_cols;

        static int num_table = 1;

        static int server_rank = 0;
        static int worker_rank = 1;

        static void Main(string[] args)
        {
            if (args.Length != 1) throw new Exception("wrong usage\n");
            if (args[0] == "worker")
            {
                // Bind a endpoint
                MultiversoWrapper.NetBind(1, worker_ip_port);
                int[] ranks = new int[1]; ranks[0] = server_rank;
                String[] ips = new String[1]; ips[0] = server_ip_port;
                // Connect with others
                MultiversoWrapper.NetConnect(ranks, ips);

                // Start workers
                MultiversoWrapper.InitWorker(num_table);

                // Create the tables (the worker table contains API to sync with server table)
                MultiversoWrapper.CreateWorkerTable(0, num_rows, num_cols, "Float");
                MultiversoWrapper.Barrier();

                // ML training logic

                float[] updates = new float[size];
                float[] data = new float[size];
                
                // some algorithm to produce the update
                for (int i = 0; i < size; ++i) updates[i] = i;


                MultiversoWrapper.Add(0, updates);

                // Get latest model from server
                MultiversoWrapper.Get(0, data);

                // CHECK the result
                // should equals to updates
                // print
                // 1 2 3 4 ...
                for (int i = 0; i < num_rows; ++i)
                {
                    Console.Write("row " + i.ToString() + " : ");
                    for (int j = 0; j < num_cols; ++j)
                    {
                        Console.Write(data[i].ToString() + " ");
                    }
                    Console.WriteLine();
                }

            }
            else if (args[0] == "server")
            {
                MultiversoWrapper.NetBind(0, server_ip_port);
                int[] ranks = new int[1]; ranks[0] = worker_rank;
                String[] ips = new String[1]; ips[0] = worker_ip_port;
                MultiversoWrapper.NetConnect(ranks, ips);

                MultiversoWrapper.InitServer(num_table);
                MultiversoWrapper.CreateServerTable(0, num_rows, num_cols, "Float");
                MultiversoWrapper.Barrier();
                // Server do nothing, just start and creat table. Then it will respond clients'
                // request, until clients call shutdown.
            }
            Console.WriteLine("Shut down...");
            MultiversoWrapper.Shutdown();
        }
    }
}
