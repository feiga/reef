#pragma once

#include "ITable.h"


namespace Org { namespace Apache { namespace REEF { namespace ParameterService {
namespace Multiverso { namespace Bridge {
  public ref class MultiversoWrapper
  {
  public:
    static bool NetBind(int rank, System::String^ endpoint);
    static bool NetConnect(array<int>^ ranks, array<System::String^>^ endpoints);
    // Worker only
    static void InitWorker(int num_tables);
    static void CreateWorkerTable(int table_id, int rows, int cols, System::String^ eleType);
    static int WorkerID();
    // Server only
    static void InitServer(int num_tables);
    static void CreateServerTable(int table_id, int rows, int cols, System::String^ eleType);
    static int ServerID();

    static void Shutdown();
    static int Rank();
    static int Size();
    static int NumWorker();
    static int NumServer();
    static void Barrier();

    static void Get(int table_id, array<int>^ p_value);
    static void Get(int table_id, array<float>^ p_value);

    static void Get(int table_id, int row_id, array<int>^ p_value);
    static void Get(int table_id, int row_id, array<float>^ p_value);

    static void Add(int table_id, array<int>^ p_update);
    static void Add(int table_id, array<float>^ p_update);

    static void Add(int table_id, int row_id, array<int>^ p_value);
    static void Add(int table_id, int row_id, array<float>^ p_value);

  private:
    static void Init(int role);
    static array<IWorkerTable^>^ worker_tables_;
    static array<IServerTable^>^ server_tables_;
    //static std::vector<IWorkerTable^>* worker_tables_;
    //static std::vector<IServerTable^>* server_tables_;
  };
}
}
}
}
}
}