#include "MultiversoClrBridge.h"
#include <multiverso/multiverso.h>

#include <vcclr.h>

using namespace System;

namespace
{
  array<char>^ ToCString(String^ csString)
  {
    array<char>^ cString = gcnew array<char>(csString->Length + 1);
    int indx = 0;
    for (; indx < csString->Length; indx++)
    {
      cString[indx] = (char)csString[indx];
    }
    cString[indx] = 0;
    return cString;
  }
}

namespace Org {
  namespace Apache {
    namespace REEF {
      namespace ParameterService {
        namespace Multiverso {
          namespace Bridge
          {

            bool MultiversoWrapper::NetBind(int rank, System::String^ endpoint) {
              array<char>^ ip_port = ToCString(endpoint);
              pin_ptr<char> ptr = &ip_port[0];
              return multiverso::MV_NetBind(rank, ptr) == 0;
            }

            bool MultiversoWrapper::NetConnect(array<int>^ ranks, array<System::String^>^ endpoints) {
              if (ranks->Length != endpoints->Length) {
                // Fatal error
              }
              pin_ptr<int> p_ranks = &ranks[0];
              array<char*>^ array_endpoints = gcnew array<char*>(endpoints->Length);
              for (int i = 0; i < endpoints->Length; ++i) {
                array<char>^ ip_port = ToCString(endpoints[i]);
                pin_ptr<char> ptr = &ip_port[0];
                array_endpoints[i] = ptr;
              }
              pin_ptr<char*> p_endpoints = &array_endpoints[0];
              return multiverso::MV_NetConnect(p_ranks, p_endpoints, ranks->Length) == 0;
            }

            void MultiversoWrapper::InitWorker(int num_tables) { 
              worker_tables_ = gcnew array<IWorkerTable^>(num_tables);
              Init(multiverso::Role::Worker); 
            }

            void MultiversoWrapper::InitServer(int num_tables) { 
              server_tables_ = gcnew array<IServerTable^>(num_tables);
              Init(multiverso::Role::Server); 
            }

            void MultiversoWrapper::CreateWorkerTable(int table_id, int rows, int cols, System::String^ eleType)
            {
              worker_tables_[table_id] = IWorkerTable::CreateTable(table_id, rows, cols, eleType);
            }

            void MultiversoWrapper::CreateServerTable(int table_id, int rows, int cols, System::String^ eleType)
            {
              server_tables_[table_id] = IServerTable::CreateTable(table_id, rows, cols, eleType);
            }

            void MultiversoWrapper::Get(int table_id, array<int>^ p_value) {
              pin_ptr<int> p = &p_value[0];
              worker_tables_[table_id]->Get(p, p_value->Length);
            }
            void MultiversoWrapper::Get(int table_id, array<float>^ p_value) {
              printf("Wrapper get\n");
              pin_ptr<float> p = &p_value[0];
              worker_tables_[table_id]->Get(p, p_value->Length);
            }

            void MultiversoWrapper::Get(int table_id, int row_id, array<int>^ p_value) {
              pin_ptr<int> p = &p_value[0];
              worker_tables_[table_id]->Get(row_id, p, p_value->Length);
            };
            void MultiversoWrapper::Get(int table_id, int row_id, array<float>^ p_value) {
              pin_ptr<float> p = &p_value[0];
              worker_tables_[table_id]->Get(row_id, p, p_value->Length);
            };

            void MultiversoWrapper::Add(int table_id, array<int>^ p_update) {
              pin_ptr<int> p = &p_update[0];
              worker_tables_[table_id]->Add(p, p_update->Length);
            }
            void MultiversoWrapper::Add(int table_id, array<float>^ p_update) {
              pin_ptr<float> p = &p_update[0];
              worker_tables_[table_id]->Add(p, p_update->Length);
            }

            void MultiversoWrapper::Add(int table_id, int row_id, array<int>^ p_update) {
              pin_ptr<int> p = &p_update[0];
              worker_tables_[table_id]->Add(row_id, p, p_update->Length);
            }

            void MultiversoWrapper::Add(int table_id, int row_id, array<float>^ p_update) {
              pin_ptr<float> p = &p_update[0];
              worker_tables_[table_id]->Add(row_id, p, p_update->Length);
            }
            //void MultiversoWrapper::Get(int table_id, array<int>^ rows, array<array<int>^> ^ p_value) {
            //  array<void*>^ array_value = gcnew array<void*>(p_value->Length);
            //  for (int i = 0; i < p_value->Length; ++i) {
            //    pin_ptr<int> p = &p_value[i][0];
            //    array_value[i] = p;
            //  }
            //  // TODO(feiga): check the type of table is is int
            //  worker_tables_[table_id]->Get(rows, array_value, p_value[0]->Length);
            //}

            void MultiversoWrapper::Shutdown() { multiverso::MV_ShutDown(); }

            int MultiversoWrapper::Rank() { return multiverso::MV_Rank(); }

            int MultiversoWrapper::Size() { return multiverso::MV_Size(); }

            int MultiversoWrapper::WorkerID() { return multiverso::MV_WorkerId(); }

            int MultiversoWrapper::ServerID() { return multiverso::MV_ServerId(); }

            int MultiversoWrapper::NumWorker() { return multiverso::MV_NumWorkers(); }

            int MultiversoWrapper::NumServer() { return multiverso::MV_NumServers(); }

            void MultiversoWrapper::Barrier() { multiverso::MV_Barrier(); }

            void MultiversoWrapper::Init(int role) { multiverso::MV_Init(nullptr, nullptr, role); }
          }
        }
      }
    }
  }
}