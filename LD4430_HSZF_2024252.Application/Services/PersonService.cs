using LD4430_HSZF_2024252.Model;
using LD4430_HSZF_2024252.Persistence.MsSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Application
{
    public class PersonService : IPersonService
    {
        private readonly IPersonDataProvider _dataProvider;

        public PersonService(IPersonDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public IEnumerable<Person> GetAllPerson()
        {
            return _dataProvider.GetAllPerson();
        }

        //Returns all person where ReponsibleForPurchase is true =>
        //the person is responsible for purchasing their favorite products
        public IEnumerable<Person> GetBuyers()
        {
            return _dataProvider
                .GetAllPerson()
                .Where(p => p.ResponsibleForPurchase);
        }

        public void AddPerson(Person person)
        {
            _dataProvider.AddPerson(person);
        }

        public void UpdatePerson(Person person)
        {
            _dataProvider.UpdatePerson(person);
        }

        public void DeletePerson(int id)
        {
            _dataProvider.DeletePerson(id);
        }

        public void DeleteAll()
        {
            _dataProvider.DeleteAll();
        }

        public Person? GetPersonById(int id)
        {
            return _dataProvider.GetPersonById(id);
        }

        public bool Exists(int personId)
        {
            return _dataProvider.Exists(personId);
        }

        public bool Exists(string personName)
        {
            return _dataProvider.Exists(personName);
        }
    }
}
