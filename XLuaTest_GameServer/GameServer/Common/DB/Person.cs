using System;
using System.Collections.Generic;
using Common.ORM;
using MongoDB.Bson.Serialization.Attributes;

namespace Common.DB
{
    [Serializable]
    public class Person : DBEntity
    {
        [BsonConstructor]
        public Person(string name, int age, string guid, GenderEnum gender)
        {
            Name = name;

            Age = age;

            Guid = guid;

            Gender = gender;
        }

        public override string ToString()
        {
            return $"ID:{ID} user:{Name} age:{Age} guid:{Guid} Gender:{Gender}";
        }

        public string Name { get; set; }

        public int Age { get; set; }

        public string Guid { get; set; }

        public GenderEnum Gender { get; set; }

        public List<Person> Students { get => students; set => students = value; }

        public Pet Pet { get => pet; set => pet = value; }

        private Pet pet;

        private List<Person> students;
    }

    public enum GenderEnum
    {
        男,

        女,
    }

    public class Pet
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }

}