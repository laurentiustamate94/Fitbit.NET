﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using Fitbit.Api.Portable;
using Fitbit.Models;
using NUnit.Framework;

namespace Fitbit.Portable.Tests
{
    [TestFixture]
    public class BloodPressureTests
    {       
        [Test]
        public async void GetBloodPressureAsync_Success()
        {
            string content = "GetBloodPressure.json".GetContent();

            var responseMessage = new Func<HttpResponseMessage>(() =>
            {
                return new HttpResponseMessage(HttpStatusCode.OK) {Content = new StringContent(content)};
            });

            var verification = new Action<HttpRequestMessage, CancellationToken>((message, token) =>
            {
                Assert.AreEqual(HttpMethod.Get, message.Method);
                Assert.AreEqual("https://api.fitbit.com/1/user/-/bp/date/2014-09-27.json", message.RequestUri.AbsoluteUri);
            });

            var fitbitClient = Helper.CreateFitbitClient(responseMessage, verification);

            var response = await fitbitClient.GetBloodPressureAsync(new DateTime(2014, 9, 27));
            Assert.IsTrue(response.Success);
            ValidateBloodPressureData(response.Data);
        }

        [Test]
        public async void GetBloodPressureAsync_Errors()
        {
            var responseMessage = Helper.CreateErrorResponse();
            var verification = new Action<HttpRequestMessage, CancellationToken>((message, token) =>
            {
                Assert.AreEqual(HttpMethod.Get, message.Method);
            });

            var fitbitClient = Helper.CreateFitbitClient(responseMessage, verification);

            var response = await fitbitClient.GetBloodPressureAsync(new DateTime(2014, 9, 27));
            Assert.IsFalse(response.Success);
            Assert.IsNull(response.Data);
            Assert.AreEqual(1, response.Errors.Count);
        }

        [Test]
        public void Can_Deserialize_Food()
        {
            string content = "GetBloodPressure.json".GetContent();
            var deserializer = new JsonDotNetSerializer();

            BloodPressureData bp = deserializer.Deserialize<BloodPressureData>(content);

            ValidateBloodPressureData(bp);
        }

        private void ValidateBloodPressureData(BloodPressureData bp)
        {
            Assert.IsNotNull(bp);

            Assert.IsNotNull(bp.Average);
            Assert.IsNotNull(bp.BP);
            
            // Average
            Assert.AreEqual("Prehypertension", bp.Average.Condition);
            Assert.AreEqual(85, bp.Average.Diastolic);
            Assert.AreEqual(115, bp.Average.Systolic);

            // bp
            var b = bp.BP.First();
            bp.BP.Remove(b);

            Assert.AreEqual(80, b.Diastolic);
            Assert.AreEqual(120, b.Systolic);
            Assert.AreEqual(DateTime.MinValue.TimeOfDay, b.Time.TimeOfDay);
            Assert.AreEqual(483697, b.LogId);

            b = bp.BP.First();
            Assert.AreEqual(90, b.Diastolic);
            Assert.AreEqual(110, b.Systolic);
            Assert.AreEqual(DateTime.MinValue.AddHours(8).TimeOfDay, b.Time.TimeOfDay);
            Assert.AreEqual(483699, b.LogId);
        }
    }
}