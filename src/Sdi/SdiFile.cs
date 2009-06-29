﻿//
// Copyright (c) 2008-2009, Kenneth Bell
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using System.Collections.Generic;
using System.IO;

namespace DiscUtils.Sdi
{
    /// <summary>
    /// Class for accessing the contents of Simple Deployment Image (.sdi) files.
    /// </summary>
    /// <remarks>SDI files are primitive disk images, containing multiple blobs.</remarks>
    public class SdiFile
    {
        private Stream _stream;
        private FileHeader _header;
        private List<SectionRecord> _sections;

        /// <summary>
        /// Creates a new instance from a stream.
        /// </summary>
        /// <param name="stream">The stream formatted as an SDI file.</param>
        public SdiFile(Stream stream)
        {
            _stream = stream;

            byte[] page = Utilities.ReadFully(_stream, 512);

            _header = new FileHeader();
            _header.ReadFrom(page, 0);


            _stream.Position = _header.PageAlignment * 512;
            byte[] toc = Utilities.ReadFully(_stream, (int)(_header.PageAlignment * 512));

            _sections = new List<SectionRecord>();
            int pos = 0;
            while (Utilities.ToUInt64LittleEndian(toc, pos) != 0)
            {
                SectionRecord record = new SectionRecord();
                record.ReadFrom(toc, pos);

                _sections.Add(record);

                pos += SectionRecord.RecordSize;
            }
        }

        /// <summary>
        /// Gets all of the sections within the file.
        /// </summary>
        public IEnumerable<Section> Sections
        {
            get
            {
                int i = 0;
                foreach (var section in _sections)
                {
                    yield return new Section(section, i++);
                }
            }
        }

        /// <summary>
        /// Opens a stream to access a particular section.
        /// </summary>
        /// <param name="index">The zero-based index of the section</param>
        /// <returns>A stream that can be used to access the section.</returns>
        public Stream OpenSection(int index)
        {
            return new SubStream(_stream, _sections[index].Offset, _sections[index].Size);
        }
    }
}