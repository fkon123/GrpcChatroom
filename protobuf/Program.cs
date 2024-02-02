using System;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Protobuf.Test;
using static Grpc.Protobuf.Test.Employee.Types;

System.Console.WriteLine("Welecome to protobuf test!");
var employee = new Employee();
employee.FirstName = "Filippos";
employee.LastName = "Kon";
employee.IsRetired = false;
var birthdate = new DateTime(1995, 8, 17);
birthdate = DateTime.SpecifyKind(birthdate, DateTimeKind.Utc);
employee.BirthDate = Timestamp.FromDateTime(birthdate);
employee.Age = 25;
employee.MaritalStatus = MaritalStatus.Single;
employee.PreviousEmployers.Add("Microsoft");
employee.PreviousEmployers.Add("Google");
employee.CurrentAddress = new Address
{
    StreetName = "123 Main St",
    HouseNumber = 123,
    City = "Seattle",
    ZipCode = "WA"
};
employee.Relatives.Add("mother","Maria");
employee.Relatives.Add("father","John");

using (var output = File.Create("employee.dat"))
{
    employee.WriteTo(output);
}

Employee empFromFile;
using (var input = File.OpenRead("employee.dat"))
{
    empFromFile = Employee.Parser.ParseFrom(input);
}

Console.WriteLine("Test completed");