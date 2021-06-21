using System;

namespace Example.Client.Wpf
{
    public class Person
    {
        public Person(int p_personId, string p_firstName, string p_lastName, DateTime p_dob, string p_phoneNumber, bool p_isLocked = false)
        {
            PersonId = p_personId;
            FirstName = p_firstName;
            LastName = p_lastName;
            Dob = p_dob;
            PhoneNumber = p_phoneNumber;
            IsLocked = p_isLocked;
        }

        public int PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Dob { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsLocked { get; set; } = false;
    }
}