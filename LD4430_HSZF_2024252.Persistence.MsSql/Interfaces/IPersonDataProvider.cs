using LD4430_HSZF_2024252.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Persistence.MsSql
{
    public interface IPersonDataProvider
    {
        IEnumerable<Person> GetAllPerson();
        Person? GetPersonById(int id);
        void AddPerson(Person person);
        void UpdatePerson(Person person);
        void DeletePerson(int id);
        void DeleteAll();
        bool Exists(int id);
        bool Exists(string Name);
    }
}
