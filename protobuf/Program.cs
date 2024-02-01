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
employee.MaritalStatus = MaritalStatus.Single;
employee.PreviousEmployers.Add("Microsoft");
employee.PreviousEmployers.Add("Google");

Console.WriteLine("Test completed");