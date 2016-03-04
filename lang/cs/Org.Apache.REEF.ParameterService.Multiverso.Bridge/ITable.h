#pragma once 

#include <multiverso/table/matrix_table.h>
#include <vector>

namespace Org { namespace Apache { namespace REEF { namespace ParameterService {
namespace Multiverso { namespace Bridge {

// Interface to mask the C++ template
interface class IWorkerTable {
public: 
  static IWorkerTable^ CreateTable(int table_id, int num_rows, int num_cols, System::String^ type); 
  virtual void Get(int row_id, void* buffer, int size) = 0;
  virtual void Get(void* buffer, int size) = 0;
  virtual void Get(array<int>^ row_ids, array<void*>^ buffers, int size) = 0;

  virtual void Add(int row_id, void* buffer, int size) = 0;
  virtual void Add(void* buffer, int size) = 0;
  virtual void Add(array<int>^ row_ids, array<void*>^ buffers, int size) = 0;
};

ref class IServerTable {
public:
  static IServerTable^ CreateTable(int table_id, int num_rows, int num_cols, System::String^ type);
  virtual ~IServerTable() {}
};

ref class WorkerIntTable : public IWorkerTable {
public:
  WorkerIntTable(int num_rows, int num_cols) : type_("Int") {
    table_ = new multiverso::MatrixWorkerTable<int>(num_rows, num_cols);
  }
  void Get(void* buffer, int size) override {
    table_->Get(static_cast<int*>(buffer), size);
  }

  void Get(int row_id, void* buffer, int size) override {
    table_->Get(row_id, static_cast<int*>(buffer), size);
  }

  void Get(array<int>^ row_ids, array<void*>^ buffers, int size) override {
    std::vector<int>  row_id_vec(size);
    std::vector<int*> buffer_vec; 
    pin_ptr<int> p = &row_ids[0];
    memcpy(row_id_vec.data(), p, size * sizeof(int));
    for (int i = 0; i < size; ++i) {
      buffer_vec.push_back(static_cast<int*>(buffers[i]));
    }
    table_->Get(row_id_vec, buffer_vec, size);
  }

  void Add(int row_id, void* buffer, int size) override {
    table_->Add(row_id, static_cast<int*>(buffer), size);

  }
  void Add(void* buffer, int size) override {
    table_->Add(static_cast<int*>(buffer), size);
  }

  void Add(array<int>^ row_ids, array<void*>^ buffers, int size) override {
    std::vector<int>  row_id_vec(size);
    std::vector<int*> buffer_vec;
    pin_ptr<int> p = &row_ids[0];
    memcpy(row_id_vec.data(), p, size * sizeof(int));
    for (int i = 0; i < size; ++i) {
      buffer_vec.push_back(static_cast<int*>(buffers[i]));
    }
    table_->Add(row_id_vec, buffer_vec, size);
  }

private:
  System::String^ type_;
  multiverso::MatrixWorkerTable<int>* table_;
};

ref class WorkerFloatTable : public IWorkerTable {
public:
  WorkerFloatTable(int num_rows, int num_cols) : type_("Float") {
    table_ = new multiverso::MatrixWorkerTable<float>(num_rows, num_cols);
  }

  void Get(void* buffer, int size) override {
    printf("before worker float table get\n");
    table_->Get(static_cast<float*>(buffer), size);
  }

  void Get(int row_id, void* buffer, int size) override {
    table_->Get(row_id, static_cast<float*>(buffer), size);
  }

  void Get(array<int>^ row_ids, array<void*>^ buffers, int size) override {
    std::vector<int>  row_id_vec(size);
    std::vector<float*> buffer_vec;
    pin_ptr<int> p = &row_ids[0];
    memcpy(row_id_vec.data(), p, size * sizeof(int));
    for (int i = 0; i < size; ++i) {
      buffer_vec.push_back(static_cast<float*>(buffers[i]));
    }
    table_->Get(row_id_vec, buffer_vec, size);
  }

  void Add(int row_id, void* buffer, int size) override {
    table_->Add(row_id, static_cast<float*>(buffer), size);

  }
  void Add(void* buffer, int size) override {
    table_->Add(static_cast<float*>(buffer), size);
  }

  void Add(array<int>^ row_ids, array<void*>^ buffers, int size) override {
    std::vector<int>  row_id_vec(size);
    std::vector<float*> buffer_vec;
    pin_ptr<int> p = &row_ids[0];
    memcpy(row_id_vec.data(), p, size * sizeof(int));
    for (int i = 0; i < size; ++i) {
      buffer_vec.push_back(static_cast<float*>(buffers[i]));
    }
    table_->Add(row_id_vec, buffer_vec, size);
  }

private:
  System::String^ type_;
  multiverso::MatrixWorkerTable<float>* table_;
};

ref class WorkerDoubleTable : public IWorkerTable {
public:
  WorkerDoubleTable(int num_rows, int num_cols) : type_("Double") {
    table_ = new multiverso::MatrixWorkerTable<double>(num_rows, num_cols);
  }

  void Get(void* buffer, int size) override {
    table_->Get(static_cast<double*>(buffer), size);
  }

  void Get(int row_id, void* buffer, int size) override {
    table_->Get(row_id, static_cast<double*>(buffer), size);
  }

  void Get(array<int>^ row_ids, array<void*>^ buffers, int size) override {
    std::vector<int>  row_id_vec(size);
    std::vector<double*> buffer_vec;
    pin_ptr<int> p = &row_ids[0];
    memcpy(row_id_vec.data(), p, size * sizeof(int));
    for (int i = 0; i < size; ++i) {
      buffer_vec.push_back(static_cast<double*>(buffers[i]));
    }
    table_->Get(row_id_vec, buffer_vec, size);
  }

  void Add(int row_id, void* buffer, int size) override {
    table_->Add(row_id, static_cast<double*>(buffer), size);

  }
  void Add(void* buffer, int size) override {
    table_->Add(static_cast<double*>(buffer), size);
  }

  void Add(array<int>^ row_ids, array<void*>^ buffers, int size) override {
    std::vector<int>  row_id_vec(size);
    std::vector<double*> buffer_vec;
    pin_ptr<int> p = &row_ids[0];
    memcpy(row_id_vec.data(), p, size * sizeof(int));
    for (int i = 0; i < size; ++i) {
      buffer_vec.push_back(static_cast<double*>(buffers[i]));
    }
    table_->Add(row_id_vec, buffer_vec, size);
  }

private:
  System::String^ type_;
  multiverso::MatrixWorkerTable<double>* table_;
};

ref class ServerIntTable : public IServerTable {
public:
  ServerIntTable(int num_rows, int num_cols) : type_("Int") {
    table_ = new multiverso::MatrixServerTable<int>(num_rows, num_cols);
  }
  ~ServerIntTable() { delete table_; }
private:
  multiverso::MatrixServerTable<int>* table_;
  System::String^ type_;
};

ref class ServerFloatTable : public IServerTable {
public:
  ServerFloatTable(int num_rows, int num_cols) : type_("Float") {
    table_ = new multiverso::MatrixServerTable<float>(num_rows, num_cols);
  }
  ~ServerFloatTable() { delete table_; }
private:
  multiverso::MatrixServerTable<float>* table_;
  System::String^ type_;
};

ref class ServerDoubleTable : public IServerTable {
public:
  ServerDoubleTable(int num_rows, int num_cols) : type_("Double") {
    table_ = new multiverso::MatrixServerTable<double>(num_rows, num_cols);
  }
  ~ServerDoubleTable() { delete table_; }
private:
  multiverso::MatrixServerTable<double>* table_;
  System::String^ type_;
};

IWorkerTable^ IWorkerTable::CreateTable(int table_id, int num_rows, int num_cols, System::String^ type) {
  if (type->Equals("Int"))    return gcnew WorkerIntTable(num_rows, num_cols);
  if (type->Equals("Float"))  return gcnew WorkerFloatTable(num_rows, num_cols);
  if (type->Equals("Double")) return gcnew WorkerDoubleTable(num_rows, num_cols);
  // TODO(feiga): add checkers
}

IServerTable^ IServerTable::CreateTable(int table_id, int num_rows, int num_cols, System::String^ type) {
  if (type->Equals("Int"))    return gcnew ServerIntTable(num_rows, num_cols);
  if (type->Equals("Float"))  return gcnew ServerFloatTable(num_rows, num_cols);
  if (type->Equals("Double")) return gcnew ServerDoubleTable(num_rows, num_cols);
  // TODO(feiga): add checkers
}

// namespace 
}
}
}
}
}
}