using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Architecture.Core.Repository;
using Architecture.Data.Entity;

namespace Architecture.Web.Controllers
{
    [Produces("application/json")]
    //[Route("api/Customer")]
    public class CustomerController : Controller
    {
        private ICustomerRepository _customerRepository;

        public CustomerController(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        [Route("api/customer/getcustomers")]
        public IList<Customer> GetCustomers()
        {
            return _customerRepository.GetAllCustomers();
        }

        [Route("api/customer/getcustomerswithsp")]
        public IList<Customer> GetCustomersWithSp()
        {
            return _customerRepository.GetAllCustomersWithSP();
        }


        [Route("api/customer/getcustomerbyid/{id}")]
        //[HttpGet("{id}")]
        [HttpGet]
        public Customer GetCustomerById(int id)
        {
            return _customerRepository.GetCustomerById(id);
        }

        [Route("api/customer/addcustomer")]
        [HttpPost]
        public void AddCustomer([FromBody] Customer customer)
        {
            _customerRepository.AddCustomer(customer);
        }

        [Route("api/customer/updatecustomer/{id}")]
        //[HttpPut("{id}")]
        [HttpPut]
        public void UpdateCustomer(int id, [FromBody] Customer customer)
        {
            _customerRepository.UpdateCustomer(id, customer);
        }

        //[HttpDelete("{id}")]
        [Route("api/customer/updatecustomer/{id}")]
        [HttpDelete]
        public void DeleteCustomer(int id)
        {
            _customerRepository.DelecteCustomer(id);
        }


    }
}
