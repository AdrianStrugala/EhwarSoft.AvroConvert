﻿using System;
using System.Collections.Generic;
using SolTechnology.Avro;
using SolTechnology.Avro.Exceptions;
using Xunit;

namespace AvroConvertTests.TwitterExample
{
    public class TwitterExampleTests
    {
        private readonly byte[] _notCompressed;
        private readonly byte[] _snappy;

        public TwitterExampleTests()
        {
            _notCompressed = System.IO.File.ReadAllBytes("TwitterExample/twitter.avro");
            _snappy = System.IO.File.ReadAllBytes("TwitterExample/twitter.snappy.avro");
        }

        [Fact]
        public void Deserialize_NotCompressed_DataIsDeserialized()
        {
            //Arrange
            List<TwitterModel> expected = new List<TwitterModel>();
            expected.Add(new TwitterModel
            {
                Timestamp = 1366150681,
                Tweet = "Rock: Nerf paper, scissors is fine.",
                Username = "miguno"
            });

            expected.Add(new TwitterModel
            {
                Timestamp = 1366154481,
                Tweet = "Works as intended.  Terran is IMBA.",
                Username = "BlizzardCS"
            });


            //Act
            var result = AvroConvert.Deserialize<List<TwitterModel>>(_notCompressed);


            //Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Deserialize_InvalidFile_InvalidAvroObjectExceptionIsThrown()
        {
            //Arrange


            //Act
            var result = Record.Exception(() => AvroConvert.Deserialize<List<TwitterModel>>(_snappy));


            //Assert
            Assert.IsType<NotSupportedException>(result);
        }
    }
}