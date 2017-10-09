﻿// Copyright 2017, Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Google.Cloud.Firestore.V1Beta1;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Dynamic;
using wkt = Google.Protobuf.WellKnownTypes;

namespace Google.Cloud.Firestore.Data.Tests
{
    internal static class SerializationTestData
    {
        internal static FirestoreDb Database { get; } = FirestoreDb.Create("proj", "db", new FakeFirestoreClient());

        public static IEnumerable<object[]> BclAndValues { get; } = new List<object[]>
        {
            { (object) null, new Value { NullValue = wkt::NullValue.NullValue } },
            // Simple types
            { true, new Value { BooleanValue = true } },
            { false, new Value { BooleanValue = false } },
            { "test", new Value { StringValue = "test" } },
            { (byte) 1, new Value { IntegerValue = 1L } },
            { (sbyte) 1, new Value { IntegerValue = 1L } },
            { (short) 1, new Value { IntegerValue = 1L } },
            { (ushort) 1, new Value { IntegerValue = 1L } },
            { 1, new Value { IntegerValue = 1L } },
            { 1U, new Value { IntegerValue = 1L } },
            { 1L, new Value { IntegerValue = 1L } },
            { 1UL, new Value { IntegerValue = 1L } },
            { 1.5F, new Value { DoubleValue = 1.5D } },
            { float.PositiveInfinity, new Value { DoubleValue = double.PositiveInfinity } },
            { float.NegativeInfinity, new Value { DoubleValue = double.NegativeInfinity } },
            { float.NaN, new Value { DoubleValue = double.NaN } },
            { 1.5D, new Value { DoubleValue = 1.5D } },
            { double.PositiveInfinity, new Value { DoubleValue = double.PositiveInfinity } },
            { double.NegativeInfinity, new Value { DoubleValue = double.NegativeInfinity } },
            { double.NaN, new Value { DoubleValue = double.NaN } },

            // Min/max values of each integer type
            { byte.MinValue, new Value { IntegerValue = byte.MinValue } },
            { byte.MaxValue, new Value { IntegerValue = byte.MaxValue } },
            { sbyte.MinValue, new Value { IntegerValue = sbyte.MinValue } },
            { sbyte.MaxValue, new Value { IntegerValue = sbyte.MaxValue } },
            { short.MinValue, new Value { IntegerValue = short.MinValue } },
            { short.MaxValue, new Value { IntegerValue = short.MaxValue } },
            { ushort.MinValue, new Value { IntegerValue = ushort.MinValue } },
            { ushort.MaxValue, new Value { IntegerValue = ushort.MaxValue } },
            { int.MinValue, new Value { IntegerValue = int.MinValue } },
            { int.MaxValue, new Value { IntegerValue = int.MaxValue } },
            { uint.MinValue, new Value { IntegerValue = uint.MinValue } },
            { uint.MaxValue, new Value { IntegerValue = uint.MaxValue } },
            { long.MinValue, new Value { IntegerValue = long.MinValue } },
            { long.MaxValue, new Value { IntegerValue = long.MaxValue } },
            // We don't cover the whole range of ulong
            { (ulong) 0, new Value { IntegerValue = 0 } },
            { (ulong) long.MaxValue, new Value { IntegerValue = long.MaxValue } },
            
            // Timestamps
            { new Timestamp(1, 500),
                new Value { TimestampValue = new wkt::Timestamp { Seconds = 1, Nanos = 500 } } },
            { new DateTime(1970, 1, 1, 0, 0, 1, DateTimeKind.Utc).AddTicks(5),
                new Value { TimestampValue = new wkt::Timestamp { Seconds = 1, Nanos = 500 } } },
            // If the local time is 1 hour ahead of UTC, the timestamp is an hour before the Unix epoch
            { new DateTimeOffset(1970, 1, 1, 0, 0, 1, TimeSpan.FromHours(1)).AddTicks(5),
                new Value { TimestampValue = new wkt::Timestamp { Seconds = 1 - 3600, Nanos = 500 } } },
            { new wkt::Timestamp { Seconds = 1, Nanos = 500 },
                new Value { TimestampValue = new wkt::Timestamp { Seconds = 1, Nanos = 500 } } },

            // Blobs
            { new byte[] { 1, 2, 3, 4 },
                new Value { BytesValue = ByteString.CopyFrom(1, 2, 3, 4) } },
            { Blob.CopyFrom(new byte[] { 1, 2, 3, 4 }),
                new Value { BytesValue = ByteString.CopyFrom(1, 2, 3, 4) } },
            { ByteString.CopyFrom(1, 2, 3, 4),
                new Value { BytesValue = ByteString.CopyFrom(1, 2, 3, 4) } },

            // GeoPoints
            { new GeoPoint(1.5, 2.5),
                new Value { GeoPointValue = new Type.LatLng { Latitude = 1.5, Longitude = 2.5 } } },
            { new Type.LatLng { Latitude = 1.5, Longitude = 2.5 },
                new Value { GeoPointValue = new Type.LatLng { Latitude = 1.5, Longitude = 2.5 } } },

            // Array values
            { new string[] { "x", "y" },
                new Value { ArrayValue = new ArrayValue { Values = { new Value { StringValue = "x" }, new Value { StringValue = "y" } } } } },
            { new List<string> { "x", "y" },
                new Value { ArrayValue = new ArrayValue { Values = { new Value { StringValue = "x" }, new Value { StringValue = "y" } } } } },
            // Deliberately ByteString rather than Blob here - we need to be able to detect the element type to perform the
            // per-element deserialization correctly
            { new List<ByteString> { ByteString.CopyFromUtf8("abc"), ByteString.CopyFromUtf8("def") },
                new Value { ArrayValue = new ArrayValue { Values = { new Value { BytesValue = ByteString.CopyFromUtf8("abc") }, new Value { BytesValue = ByteString.CopyFromUtf8("def") } } } } },

            // Map values (that can be deserialized again): dictionaries, attributed types, expandos (which are just dictionaries)
            { new Dictionary<string, object> { { "name", "Jon" }, { "score", 10L } },
                new Value { MapValue = new MapValue { Fields = { { "name", new Value { StringValue = "Jon" } }, { "score", new Value { IntegerValue = 10L } } } } } },
            { new GameResult { Name = "Jon", Score = 10 },
                new Value { MapValue = new MapValue { Fields = { { "name", new Value { StringValue = "Jon" } }, { "Score", new Value { IntegerValue = 10L } } } } } },
            { () => { dynamic d = new ExpandoObject(); d.name = "Jon"; d.score = 10L; return d; },
                new Value { MapValue = new MapValue { Fields = { { "name", new Value { StringValue = "Jon" } }, { "score", new Value { IntegerValue = 10L } } } } } },
            // Nullable type handling
            { new NullableContainer { NullableValue = null },
                new Value { MapValue = new MapValue { Fields = { { "NullableValue", new Value { NullValue = wkt.NullValue.NullValue } } } } } },
            { new NullableContainer { NullableValue = 10 },
                new Value { MapValue = new MapValue { Fields = { { "NullableValue", new Value { IntegerValue = 10L } } } } } },

            // Document references
            { Database.Document("a/b"),
                new Value { ReferenceValue = "projects/proj/databases/db/documents/a/b" } },
        };

        // Only equatable for the sake of testing; that's not a requirement of the serialization code.
        [FirestoreData]
        internal class GameResult : IEquatable<GameResult>
        {
            [FirestoreProperty("name")]
            public string Name { get; set; }
            [FirestoreProperty] // No property name specified, so field will be Score
            public int Score { get; set; }

            public override int GetHashCode() => Name.GetHashCode() ^ Score;

            public override bool Equals(object obj) => Equals(obj as GameResult);

            public bool Equals(GameResult other) => other != null && other.Name == Name && other.Score == Score;
        }

        [FirestoreData]
        internal class NullableContainer : IEquatable<NullableContainer>
        {
            [FirestoreProperty]
            public int? NullableValue { get; set; }

            public override int GetHashCode() => NullableValue ?? 0;

            public override bool Equals(object obj) => Equals(obj as NullableContainer);

            public bool Equals(NullableContainer other) => other != null && other.NullableValue == NullableValue;
        }
    }
}