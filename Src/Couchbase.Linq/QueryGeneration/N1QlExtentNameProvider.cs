﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remotion.Linq.Clauses;

namespace Couchbase.Linq.QueryGeneration
{
    /// <summary>
    /// Provides unique names to use in N1QL queries for each IQuerySource extent
    /// </summary>
    public class N1QlExtentNameProvider
    {
        private const string ExtentNameFormat = "Extent{0}";

        private int _extentIndex = 0;
        private readonly Dictionary<IQuerySource, string> _extentDictionary = new Dictionary<IQuerySource, string>();

        /// <summary>
        /// Provides the extent name for a given query source
        /// </summary>
        /// <param name="querySource">IQuerySource for which to get the extent name</param>
        /// <returns>The unescaped extent name for the N1QL query</returns>
        public string GetExtentName(IQuerySource querySource)
        {
            if (querySource == null)
            {
                throw new ArgumentNullException("querySource");
            }

            string extentName;
            if (!_extentDictionary.TryGetValue(querySource, out extentName))
            {
                extentName = GetNextExtentName();

                _extentDictionary.Add(querySource, extentName);
            }

            return extentName;
        }

        /// <summary>
        /// Links two extents together so they share the same name
        /// </summary>
        /// <param name="primaryExtent">Extent to link to, which may or may not already have a name</param>
        /// <param name="secondaryExtent">New extent to share the name of the primaryExtent</param>
        /// <returns>
        /// Primarily used when handling join clauses that join to subqueries.  This allows the subquery from
        /// clause to share the same name as the join clause itself, since they are being merged into a single
        /// join clause in the N1QL query output.
        /// </returns>
        public void LinkExtents(IQuerySource primaryExtent, IQuerySource secondaryExtent)
        {
            if (primaryExtent == null)
            {
                throw new ArgumentNullException("primaryExtent");
            }
            if (secondaryExtent == null)
            {
                throw new ArgumentNullException("secondaryExtent");
            }

            if (_extentDictionary.ContainsKey(secondaryExtent))
            {
                throw new InvalidOperationException("The given secondaryExtent has already been generated a unique extent name");
            }

            _extentDictionary.Add(secondaryExtent, GetExtentName(primaryExtent));
        }

        /// <summary>
        /// Generates a one-time use extent name, which isn't linked to an IQuerySource
        /// </summary>
        /// <returns>The unescaped extent name for the N1QL query</returns>
        public string GetUnlinkedExtentName()
        {
            return GetNextExtentName();
        }

        private string GetNextExtentName()
        {
            return string.Format(ExtentNameFormat, ++_extentIndex);
        }
    }
}