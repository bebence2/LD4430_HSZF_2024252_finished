using LD4430_HSZF_2024252.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Persistence.MsSql
{
    public class PersonDataProvider : IPersonDataProvider
    {
        private readonly AppDbContext _context;

        public PersonDataProvider(AppDbContext context)
        {
            _context = context;
        }
        //Create
        public void AddPerson(Person person)
        {
            _context.Person.Add(person);
            _context.SaveChanges();
        }

        //Read
        public IEnumerable<Person> GetAllPerson()
        {
            return _context.Person.ToList();
        }
        public Person? GetPersonById(int id)
        {
            return _context.Person.FirstOrDefault(p => p.Id == id);
        }
        
        //Update
        public void UpdatePerson(Person person)
        {
            _context.Person.Update(person);
            _context.SaveChanges();
        }
        //Delete
        public void DeletePerson(int id)
        {
            var person = _context.Person.Find(id);
            if (person != null)
            {
                _context.Person.Remove(person);
                _context.SaveChanges();
            }
        }
        public void DeleteAll()
        {
            var allPersons = _context.Person.ToList();
            _context.Person.RemoveRange(allPersons);
            _context.SaveChanges();
        }

        

        public bool Exists(int id)
        {
            return _context.Person.Any(p => p.Id == id);
        }

        public bool Exists(string name)
        {
            return _context.Person.Any(p => p.Name == name);
        }

        
    }
}
