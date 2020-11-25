using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Architecture.Data.Entity;

namespace Architecture.Core.Repository
{
    public interface ICustomerRepository
    {
        IList<Customer> GetAllCustomers();
        IList<Customer> GetAllCustomersWithSP();
        Customer GetCustomerById(int idCustomer);
        void AddCustomer(Customer customer);
        void UpdateCustomer(int id, Customer customer);
        void DelecteCustomer(int idCustomer);
    }
}
