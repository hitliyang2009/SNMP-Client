﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.QualityTools.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnmpClient;
using SnmpSharpNet;

namespace SnmpClient
{
    /// <summary>
    /// Klasa do testowania funkcjonalności klasy AgentSNMP
    /// </summary>
    [TestClass]
    public class SNMP_Agent_Tester
    {
        [TestMethod]
        public void setUp()
        {
            SNMP_Agent agent = new SNMP_Agent();
            Assert.AreEqual(agent.snmp.Community, "public");
            Assert.AreEqual(agent.snmp.PeerName, "localhost");
            Assert.IsTrue(agent.snmp.Valid);
        }

        [TestMethod]
        public void TestGetRequest1()
        {
            SNMP_Agent agent = new SNMP_Agent("127.0.0.1", "public");
            string oid = "1.3.6.1.2.1.2.2.1.17.1";
            IPAddress IP = IPAddress.Parse("127.0.0.1");

            var result = agent.GetRequest(SnmpVersion.Ver1, oid, IP);

            // Pdu class used for all requests
            Pdu pdu = new Pdu(PduType.Get);

            pdu.VbList.Add(oid); //sysDescr

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Version, SnmpVersion.Ver1);

            //VarBindListy pdu zwracanego i wygenerowanego w tym tescie powinny byc inne
            Assert.AreNotEqual(result.Pdu.VbList, pdu.VbList);

            Console.WriteLine(result.Pdu.VbList[0].Value.ToString());
        }

        [TestMethod]
        public void TestGetNextRequest1()
        {
            SNMP_Agent agent = new SNMP_Agent("127.0.0.1", "public");
            string oid = "1.3.6.1.2.1.2.2.1.17.1";
            IPAddress IP = IPAddress.Parse("127.0.0.1");

            var result = agent.GetNextRequest(SnmpVersion.Ver1, oid, IP);

            // Pdu class used for all requests
            Pdu pdu = new Pdu(PduType.GetNext);

            pdu.VbList.Add(oid); //sysDescr

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Version, SnmpVersion.Ver1);

            //VarBindListy pdu zwracanego i wygenerowanego w tym tescie powinny byc inne
            Assert.AreNotEqual(result.Pdu.VbList, pdu.VbList);

            Console.WriteLine(result.Pdu.VbList[0].Value.ToString());
        }

        [TestMethod]
        public void testGetTable()
        {
            SNMP_Agent agent = new SNMP_Agent("localhost", "public");

            //tcpConnTable
            string oid = ".1.3.6.1.2.1.4.20.1";

            //IPAddress IP = IPAddress.Parse("127.0.0.1");

            var result = agent.GetTableRequest(SnmpVersion.Ver2, oid);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count!=0);

            //Wyświetlanie tabeli
            foreach (var key in result)
            {
                var dictionary = key.Value;

                foreach (var KEY in dictionary)
                {
                    Console.WriteLine("Column: " + KEY.Key + " Value: " + KEY.Value);
                }
            }
        }

        [TestMethod]
        public void testWebsiteExample()
        {
            // SNMP community name
            OctetString community = new OctetString("public");

            // Define agent parameters class
            AgentParameters param = new AgentParameters(community);
            // Set SNMP version to 1 (or 2)
            param.Version = SnmpVersion.Ver1;
            // Construct the agent address object
            // IpAddress class is easy to use here because
            //  it will try to resolve constructor parameter if it doesn't
            //  parse to an IP address
            IpAddress agent = new IpAddress("127.0.0.1");

            // Construct target
            UdpTarget target = new UdpTarget((IPAddress)agent, 161, 2000, 1);

            // Pdu class used for all requests
            Pdu pdu = new Pdu(PduType.Get);
            pdu.VbList.Add("1.3.6.1.2.1.1.1.0"); //sysDescr
            pdu.VbList.Add("1.3.6.1.2.1.1.2.0"); //sysObjectID
            pdu.VbList.Add("1.3.6.1.2.1.1.3.0"); //sysUpTime
            pdu.VbList.Add("1.3.6.1.2.1.1.4.0"); //sysContact
            pdu.VbList.Add("1.3.6.1.2.1.1.5.0"); //sysName

            // Make SNMP request
            SnmpPacket result = (SnmpPacket)target.Request(pdu, param);

            Assert.IsNotNull(result);
            // If result is null then agent didn't reply or we couldn't parse the reply.
            if (result != null)
            {
                // ErrorStatus other then 0 is an error returned by 
                // the Agent - see SnmpConstants for error definitions
                if (result.Pdu.ErrorStatus != 0)
                {
                    // agent reported an error with the request
                    Console.WriteLine("Error in SNMP reply. Error {0} index {1}",
                        result.Pdu.ErrorStatus,
                        result.Pdu.ErrorIndex);
                }
                else
                {
                    // Reply variables are returned in the same order as they were added
                    //  to the VbList
                    Console.WriteLine("sysDescr({0}) ({1}): {2}",
                        result.Pdu.VbList[0].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[0].Value.Type),
                        result.Pdu.VbList[0].Value.ToString());
                    Console.WriteLine("sysObjectID({0}) ({1}): {2}",
                        result.Pdu.VbList[1].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[1].Value.Type),
                        result.Pdu.VbList[1].Value.ToString());
                    Console.WriteLine("sysUpTime({0}) ({1}): {2}",
                        result.Pdu.VbList[2].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[2].Value.Type),
                        result.Pdu.VbList[2].Value.ToString());
                    Console.WriteLine("sysContact({0}) ({1}): {2}",
                        result.Pdu.VbList[3].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[3].Value.Type),
                        result.Pdu.VbList[3].Value.ToString());
                    Console.WriteLine("sysName({0}) ({1}): {2}",
                        result.Pdu.VbList[4].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[4].Value.Type),
                        result.Pdu.VbList[4].Value.ToString());
                }
            }
            else
            {
                Console.WriteLine("No response received from SNMP agent.");
            }
            target.Close();
        }

        [TestMethod]
        public void testMatchOid()
        {
            string[] OID = {".1.3.6.1.2.1.4.20.1.1.2.3.4.1.2", ".1.3.6.1.2.1.4.20.1.1.2.3.4.1", ".1.3.6.1.2.1.4.20.1"};
            string OID_Trunk = ".1.3.6.1.2.1.4.20.1";
            StringBuilder PatternBuilder = new StringBuilder(OID_Trunk);
            PatternBuilder.Append(@"(.\d+)+|");
            PatternBuilder.Append(OID_Trunk);
            Assert.AreEqual(PatternBuilder.ToString(), OID_Trunk + @"(.\d+)+|" + OID_Trunk);
            Console.WriteLine(PatternBuilder.ToString());

            string longestMatch = "";

            for (int i = 0; i < OID.Length; i++)
            {
                var match = Regex.Match(OID[i], PatternBuilder.ToString(), RegexOptions.None);
                Assert.IsTrue(match.Success);

                if (longestMatch.Length < match.Value.Length)
                {
                    longestMatch = match.Value;
                    Assert.AreEqual(match.Value, longestMatch);
                }
            }

            Assert.AreEqual(".1.3.6.1.2.1.4.20.1.1.2.3.4.1.2", longestMatch);

            Console.WriteLine("Longest match is: " + longestMatch);
        }

        [TestMethod]
        public void testMatchTrunk()
        {
            List<ExtensionDictionary> OID_List = new List<ExtensionDictionary>();
            string[] OIDs = { ".1.3.6.1.2.1.4.20.1", ".1.3.6.1.2.1.4.20.1.3",
                ".1.3.6.1.2.1.4.20.1.3.5", ".1.3.6.1.2.1.4.20.1.3.5.7", 
                /*niepasujący*/ ".1.3.6.1.4.2.5.3.5", ".1.3.6.1.2.1.4.20.1.3.5.7.9.0"};

            for (int i = 0; i < OIDs.Length; i++)
            {
                OID_List.Add(new ExtensionDictionary(i.ToString(), OIDs[i], null));
            }

            //Szukane OID do dopasowania
            string longOid = ".1.3.6.1.2.1.4.20.1.3.5.7.9.0";

            //Najdluzsze dopasowanie
            string longestMatch = ".1.3.6.1.2.1.4.20.1.3.5.7.9.0";

            //Dopasowanie zwrocone przez funkcje
            string returnedLongestMatch = Form1.findLongestTrunk(OID_List, longOid);

            Assert.AreEqual(longestMatch, returnedLongestMatch);
        }

    }
}
