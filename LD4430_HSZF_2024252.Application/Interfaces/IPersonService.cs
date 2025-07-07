using LD4430_HSZF_2024252.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Application
{
    public interface IPersonService
    {
        IEnumerable<Person> GetAllPerson();
        IEnumerable<Person> GetBuyers(); //Returns all person where ReponsibleForPurchase is true => the person is responsible for purchasing their favorite products
        void AddPerson(Person person);
        void UpdatePerson(Person person);
        void DeletePerson(int id);
        void DeleteAll();
        bool Exists(int personId);
        bool Exists(string personName);
        Person? GetPersonById(int id);
    }
}
