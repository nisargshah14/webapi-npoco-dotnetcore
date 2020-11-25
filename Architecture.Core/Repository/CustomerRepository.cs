using System;
using System.Collections.Generic;
using System.Text;
using NPoco;
using System.Data.SqlClient;
using Architecture.Data.Entity;
using Architecture.Data.Repository;

namespace Architecture.Core.Repository
{
    public class CustomerRepository : BaseRepository, ICustomerRepository
    {
        //Connection Object  


        public IList<Customer> GetAllCustomers()
        {
            //string query = "SELECT * FROM Customers";
            IList<Customer> customerList = GetEntityList<Customer>(); //connection.Fetch<Customer>(query);

            return customerList;
        }

        public IList<Customer> GetAllCustomersWithSP()
        {
            //string query = "SELECT * FROM Customers";
            IList<Customer> customerList = GetEntityList<Customer>("GetCustomerList", null);

            return customerList;
        }

        public Customer GetCustomerById(int idCustomer)
        {
            Customer customer = GetEntity<Customer>(idCustomer); //connection.SingleById<Customer>(idCustomer);
            return customer;
        }

        public void AddCustomer(Customer customer)
        {
            SaveEntity(customer, 0, false);
            //connection.Insert<Customer>(customer);
        }

        public void UpdateCustomer(int id, Customer customer)
        {
            customer.CustomerId = id;
            SaveEntity(customer, 0, false);
            //connection.Update(customer);
        }

        public void DelecteCustomer(int idCustomer)
        {
            DeleteEntity<Customer>(idCustomer);
            //connection.Delete<Customer>(idCustomer);
        }

    }
}
