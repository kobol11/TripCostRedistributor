/************************************************************************************************** 
 * Program Name: Trip Expense Redistributor
 * Author: Bolarinwa Komolafe
 * Version: 1.0.0
 * Date: August 30, 2018
 * *************************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripCostRedistributor
{
    class Program
    {
        static void Main(string[] args)
        {
            // Ensure input file path is provided
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the path for the input text file.");
                return;
            }
            string inputFilePath = args[0];
            string outputFilePath = inputFilePath + ".out";
            FileStream fs = null;
            CampingBill bill = new CampingBill();
            bool lastTrip = false;
            int tripCount = 0;
            //Variable "trips" stores the participants associated with each trip
            //and the expense(s) incurred by each participant in a Dictionary
            Dictionary<int, List<Participant>> trips = new Dictionary<int, List<Participant>>();

            try
            {
                #region input file reading and processing
                //Input file reading and processing starts here.

                fs = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read);
                StreamReader expenses = new StreamReader(fs);
                List<string> allExpenses = new List<string>();
                while (expenses.Peek() != -1) //check for end of file
                {
                    allExpenses.Add(expenses.ReadLine()); //read the file once for efficiency purpose.
                }
                if (allExpenses.Count > 0)
                {
                    int index = 0; // it holds the index of the next element of variable "allExpenses"
                    // to be accessed and processed
                    while (!lastTrip)
                    {

                        int participantsCount = bill.IntegerParser(allExpenses[index++]);
                        // check to make sure the input file entry is not corrupted
                        if (participantsCount == -1)
                        {
                            trips = null;
                            return;
                        }
                        if (participantsCount != 0)
                        {
                            tripCount++;
                            List<Participant> participants = new List<Participant>();
                            for (int i = 0; i < participantsCount; i++)
                            {
                                int expenseCount = bill.IntegerParser(allExpenses[index++]);
                                if (expenseCount == -1)
                                {
                                    participants = null;
                                    trips = null;
                                    return;
                                }
                                // Instantiate a new Participant object
                                Participant P = new Participant();
                                P.TripID = tripCount;
                                for (int j = 0; j < expenseCount; j++)
                                {
                                    double expense = bill.DoubleParser(allExpenses[index++]);
                                    if ((int)expense == -1)
                                    {
                                        participants = null;
                                        P = null;
                                        trips = null;
                                        return;
                                    }
                                    P.Expenses.Add(expense); //associate this expense to the participant object
                                }

                                //Add Participant P to the List of participants involved in this trip.
                                participants.Add(P); 
                            }

                            //Add all participants involved in a particular trip to the Dictionary.
                            trips.Add(tripCount, participants);
                        }
                        else
                        {
                            lastTrip = true;
                        }
                    }
                }
                #endregion

                #region Cost redistribution calculation
                // Cost redistribution calculation starts here.
                if (trips.Count > 0)
                {
                    StreamWriter cost_per_participant = new StreamWriter(new FileStream(outputFilePath, FileMode.Create, FileAccess.Write));
                    foreach (KeyValuePair<int, List<Participant>> kvp in trips)
                    {
                        double totalCost = 0.0; //total amount spent on a particula trip.
                        foreach (Participant participant in kvp.Value)
                        {
                            participant.TotalMoneySpent = bill.TotalCost(participant);
                            totalCost += participant.TotalMoneySpent;
                        }

                        double averageCost = totalCost / kvp.Value.Count; //cost per participant per trip.

                        foreach (Participant participant in kvp.Value)
                        {
                            participant.Refund = bill.Refund(participant, averageCost);
                        }
                        #endregion

                        #region Write cost per participant per trip to output file.
                        foreach (Participant participant in kvp.Value)
                        {
                            if (participant.Refund < 0)
                            {
                                double output = Math.Abs(participant.Refund);
                                cost_per_participant.WriteLine(String.Format("(${0:0.00})", output));
                            }
                            else
                                cost_per_participant.WriteLine(String.Format("${0:0.00}", participant.Refund));
                        }
                        cost_per_participant.WriteLine();
                        #endregion
                    }
                    cost_per_participant.Close();
                }

            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Sorry, the input file does not exit.");
                return;
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("Sorry, the file directory does not exist.");
                return;
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }
    }

    #region Participant
    // This class is the template for a typical participant in a trip.
    public class Participant
    {
        public int TripID { get; set; }
        public List<double> Expenses { get; set; } // the List of all expenses incurred by a participant.
        public double Refund { get; set; } //this property holds the amount a participant owes the group. 
        public double TotalMoneySpent { get; set; } // total amount spent by a participant on the trip.
        public Participant()
        {
            TripID = 0;
            Expenses = new List<double>();
            Refund = 0.00;
            TotalMoneySpent = 0.00;
        }
    }
    #endregion

    #region CampingBill
    // This class contains methods for processing the cost associated with each trip.
    public class CampingBill
    {
        #region IntegerParser
        // This method parses a string to an integer.
        public int IntegerParser(string str)
        {
            int output;
            if (int.TryParse(str.Trim(), out output))
            {
                return output;
            }
            else
            {
                Console.WriteLine("Sorry, the input file is corrupted.");
                return -1;
            }
        }
        #endregion

        #region DoubleParser
        // This method parses a string to a double.
        public double DoubleParser(string str)
        {
            double output;
            if (double.TryParse(str.Trim(), out output))
            {
                return output;
            }
            else
            {
                Console.WriteLine("Sorry, the input file is corrupted.");
                return -1;
            }
        }
        #endregion

        #region TotalCost
        // This method calculates the total cost of all services paid for by a trip participant.
        public double TotalCost(Participant participant)
        {
            double totalCost = 0.0;

            foreach (double expense in participant.Expenses)
            {
                totalCost += expense;
            }

            return totalCost;
        }
        #endregion

        #region Refund
        // This method calculates the amount each trip participant owes the group.
        public double Refund(Participant participant, double averageCost)
        {
            double totalCost = 0.0;

            totalCost = averageCost - participant.TotalMoneySpent;

            return totalCost;
        }
        #endregion
    }
    #endregion
}