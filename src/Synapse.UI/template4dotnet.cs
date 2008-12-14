/*
 * TemplateEngine for .NET
 * Copyright (C) 2003 Niels Wojciech Tadeusz Andersen <haj@zhat.dk>
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */


using System;
using System.IO;
using System.Text;
using System.Reflection;

namespace TemplateEngine
{
	public class Template
	{
		#region Template field and section definitions	
		private const string DELIMITER = "@@";
		private const string SECTIONTAG_HEAD = "<!-- @@";
		private const string SECTIONTAG_TAIL = "@@ -->";
		#endregion
		
		private const int DELIMITER_LEN = 2;
		private const int SECTIONTAG_HEAD_LEN = 7;
		private const int SECTIONTAG_TAIL_LEN = 6;
	
		// Hash table size. 
		// Number of fields or sections will probably be less than 10
		private const int MAX_FIELDS = 31;
	
		// Create an empty initialized template
		public Template()
		{
			head = new node();
			addedHead = new node();
			addedTail = addedHead;
			firstSection = new section();
			tpl = this;
			fields = new Object[MAX_FIELDS];
			sections = new Object[MAX_FIELDS];
		}
	
		// Create using template file
		public Template(string data)
		{
			head = new node();
			addedHead = new node();
			addedTail = addedHead;
			firstSection = new section();
			tpl = this;
			fields = new Object[MAX_FIELDS];
			sections = new Object[MAX_FIELDS];
			
			data = SECTIONTAG_HEAD + data + SECTIONTAG_TAIL;
			construct(data, SECTIONTAG_HEAD_LEN, data.Length-SECTIONTAG_TAIL_LEN);
		}
	
		// Copy ctor
		public Template(Template srctpl)
		{
			tpl = this;
			fields = new Object[MAX_FIELDS];
			sections = new Object[MAX_FIELDS];
	
			head = new node();
			addedHead = new node();
			addedTail = addedHead;
			firstSection = new section();
	
			node tail = head;
			node currNode = srctpl.head;
			section lastSection = firstSection;
			section currSection = srctpl.firstSection.nextSection;
	
			for (; currSection != null; currSection = currSection.nextSection)
			{
				// Copy part before section
				while (currNode != currSection.preceding)
				{
					currNode = currNode.next;
					tail.next = new node();
					tail = tail.next;
					if (currNode.val.shared)
						tail.val = _produceField(((field)currNode.val).key);
					else
						tail.val = new cell();
					tail.val.val = currNode.val.val;
				}
	
				// Create section entry
				lastSection.nextSection = _produceSection(currSection.key);
				lastSection = lastSection.nextSection;
				lastSection.preceding = tail;
				lastSection.tpl = new Template(currSection.tpl);
				lastSection.tpl.parent = this;
				
				// Copy added content
				if (currSection.tpl.addedHead.next != null)
				{
					int len = 0;
					for (node n=currSection.tpl.addedHead.next; n!=null; n=n.next)
						len += n.val.val.Length;
					lastSection.tpl.addedTail.next = new node();
					lastSection.tpl.addedTail = lastSection.tpl.addedTail.next;
					lastSection.tpl.addedTail.val = new cell();
	
					// Make content string
					StringBuilder sb = new StringBuilder(len);
					for (node n=currSection.tpl.addedHead.next; n!=null; n=n.next)
						sb.Append(n.val.val);
	
					lastSection.tpl.addedTail.val.val = sb.ToString();
				}
			}
			// Copy rest
			while ((currNode = currNode.next) != null)
			{
				tail.next = new node();
				tail = tail.next;
				if (currNode.val.shared)
					tail.val = _produceField(((field)currNode.val).key);
				else
					tail.val = new cell();
				tail.val.val = currNode.val.val;
			}		
		}
	
		// This is private method, but it shows how the thing gets created using recursion
		// so I let it stay close to top.
		private void construct(string data, int oLast, int oEnd)
		{
			node tail = head;
			section lastSection = firstSection;
			int oCurr, oNext;
	
			while ((oCurr = data.IndexOf(DELIMITER, oLast)) != -1 && oCurr < oEnd)
			{
				// Move past delimiter
				oCurr += DELIMITER_LEN;
	
				// Find matching delimiter
				if ((oNext = data.IndexOf(DELIMITER, oCurr)) == -1 || oNext > oEnd)
					throw new Exception("bad syntax: missing delimiter");
	
				// Section
				if (String.Compare(SECTIONTAG_HEAD, 0, data, oCurr-SECTIONTAG_HEAD_LEN, SECTIONTAG_HEAD_LEN) == 0
					&& String.Compare(SECTIONTAG_TAIL, 0, data, oNext, SECTIONTAG_TAIL_LEN) == 0)
				{
					// If there is any text since last field or section tag
					if (oCurr - oLast > SECTIONTAG_HEAD_LEN)
					{
						tail.next = new node();
						tail = tail.next;
						tail.val = new cell();
						tail.val.val = data.Substring(oLast, oCurr-SECTIONTAG_HEAD_LEN-oLast);
					}
					string tag = data.Substring(oCurr-SECTIONTAG_HEAD_LEN,oNext-oCurr+SECTIONTAG_HEAD_LEN+SECTIONTAG_TAIL_LEN);
					section s = _produceSection(data.Substring(oCurr,oNext-oCurr));
					oLast = oNext + SECTIONTAG_TAIL_LEN;
					oNext = data.IndexOf(tag, oLast);
					if (oNext == -1 || oNext > oEnd)
						throw new Exception("Bad syntax: Missing section end tag");
					if (s.preceding != null)
						throw new Exception("Bad syntax: Duplicate section");
					s.preceding = tail;
					s.tpl = new Template();
					s.tpl.construct(data, oLast, oNext);
					s.tpl.parent = this;
					lastSection.nextSection = s;
					lastSection = s;
					oLast = oNext + tag.Length;			
				}
				// Field
				else
				{
					if (oCurr - oLast > DELIMITER_LEN)
					{
						tail.next = new node();
						tail = tail.next;
						tail.val = new cell();
						tail.val.val = data.Substring(oLast, oCurr-DELIMITER_LEN-oLast);
					}
					field f = _produceField(data.Substring(oCurr,oNext - oCurr));
					tail.next = new node();
					tail = tail.next;
					tail.val = f;
					oLast = oNext + DELIMITER_LEN;
				}
			}
			if (oLast < oEnd)
			{
				// Save rest of text
				tail.next = new node();
				tail.next.val = new cell();
				tail.next.val.val = data.Substring(oLast, oEnd-oLast);
			}
		}
	
		public void reset()
		{
			for (int i=0; i<MAX_FIELDS; i++)
			{
				for (field f = (field)fields[i]; f != null; f = f.next)
					f.val = "";
			}
			for (section s = firstSection.nextSection; s != null; s = s.nextSection)
			{
				s.tpl.addedHead.next = null;
				s.tpl.addedTail = s.tpl.addedHead;
				s.tpl.reset();
			}
		}
	
		public void SetField(string key, string val)
		{
			if (val == null)
				throw new ArgumentNullException("val");
			
			field f = tpl._getField(key);
			if (f != null)
				f.val = val;
			else
				throw new Exception("Key not found");
		}
	
		public void SetFieldGlobal(string key, string val)
		{
			SetField(key, val);
			for (section s=firstSection.nextSection; s!=null; s=s.nextSection)
				s.tpl.SetFieldGlobal(key, val);
		}
	
		public void SetFieldFromFile(string key, string filename)
		{
			field f = _getField(key);
			if (f != null)
			{
				StreamReader re = File.OpenText(filename);
				f.val = re.ReadToEnd();
				re.Close();
			}
		}
	
		public void SelectSection(string key)
		{
			section s = tpl._getSection(key);
			if (s == null)
				throw new Exception("SelectSection: Cannot find section "+key);
			tpl.tpl = s.tpl;
			tpl = tpl.tpl;
		}
	
		public void DeselectSection()
		{
			if (tpl == this)
				throw new Exception("DeselectSection: No section selected");
			tpl = tpl.parent;
		}
	
		// This is definitely the part that beats C code in speed.
		// Allocating memory is very fast in .NET
		public void AppendSection()
		{
			if (tpl == this)
				throw new Exception("AppendSection: No section selected");
			node currNode = tpl.head;
			for (section currSection=tpl.firstSection.nextSection; currSection!=null; currSection=currSection.nextSection)
			{
				while (currNode != currSection.preceding)
				{
					currNode = currNode.next;
					tpl.addedTail.next = new node();
					tpl.addedTail = tpl.addedTail.next;
					tpl.addedTail.val = new cell();
					tpl.addedTail.val.val = currNode.val.val;
				}
				if (currSection.tpl.addedHead.next != null)
				{
					tpl.addedTail.next = currSection.tpl.addedHead.next;
					tpl.addedTail = currSection.tpl.addedTail;
					currSection.tpl.addedHead.next = null;
					currSection.tpl.addedTail = currSection.tpl.addedHead;
				}
			}
			while ((currNode = currNode.next) != null)
			{
				tpl.addedTail.next = new node();
				tpl.addedTail = tpl.addedTail.next;
				tpl.addedTail.val = new cell();
				tpl.addedTail.val.val = currNode.val.val;
			}
		}
	
		public void attachSection(string key)
		{
			SelectSection(key);
			AppendSection();
			DeselectSection();
		}
	
		public void setSection(string key, string data)
		{
			section s = tpl._getSection(key);
			if (s == null)
				throw new Exception("setSection: Cannot find section "+key);
			node n = new node();
			n.val = new cell();
			n.val.val = data;
			n.next = s.preceding.next;
			s.preceding.next = n;
			s.preceding = n;		
		}
	
		public void setSectionFromFile(string key, string filename)
		{
			section s = tpl._getSection(key);
			if (s == null)
				throw new Exception("setSection: Cannot find section "+key);
			node n = new node();
			n.val = new cell();
			StreamReader re = File.OpenText(filename);
			n.val.val = re.ReadToEnd();
			re.Close();
			n.next = s.preceding.next;
			s.preceding.next = n;
			s.preceding = n;		
		}
	
		public string getSection(string key)
		{
			section s = tpl._getSection(key);
			if (s == null)
				throw new Exception("getSection: Cannot find section "+key);
			return s.tpl.getContent();
		}
	
		public string getContent()
		{
			int len = 0;
	
			// Traverse this' content to get length
			for (node n=head.next; n!=null; n=n.next)
				len += n.val.val.Length;
			
			// Traverse all sections to add length of content added to them
			for (section currSection=firstSection.nextSection; currSection!=null; currSection=currSection.nextSection)
			{
				for (node n=currSection.tpl.addedHead.next; n!=null; n=n.next)
					len += n.val.val.Length;
			}
	
			// Traverse to get content in the right order
			node currNode = head;
			StringBuilder sb = new StringBuilder(len);
			for (section currSection=firstSection.nextSection; currSection!=null; currSection=currSection.nextSection)
			{
				while (currNode != currSection.preceding)
				{
					currNode = currNode.next;
					sb.Append(currNode.val.val);
				}
				for (node n=currSection.tpl.addedHead.next; n!=null; n=n.next)
					sb.Append(n.val.val);			
			}
			while ((currNode = currNode.next) != null)
					sb.Append(currNode.val.val);
			return sb.ToString();
		}
	
		/*
		 *	Private methods
		 */
	
		private field _getField(string key)
		{
			field f = (field)fields[(uint)(key.GetHashCode()) % MAX_FIELDS];
			while (f != null)
			{
				if (f.key == key)
					return f;
				f = f.next;
			}
			return null;
		}
		private field _produceField(string key)
		{
			uint pos = (uint)(key.GetHashCode()) % MAX_FIELDS;
			field f = (field)fields[pos];
			while (f != null)
			{
				if (f.key == key)
					return f;
				f = f.next;
			}
			f = new field(key);
			f.next = (field)fields[pos];
			fields[pos] = f;
			return f;
		}
		private section _getSection(string key)
		{
			section s = (section)sections[(uint)(key.GetHashCode()) % MAX_FIELDS];
			while (s != null)
			{
				if (s.key == key)
					return s;
				s = s.next;
			}
			return null;
		}
		private section _produceSection(string key)
		{
			uint pos = (uint)(key.GetHashCode()) % MAX_FIELDS;
			section s = (section)sections[pos];
			while (s != null)
			{
				if (s.key == key)
					return s;
				s = s.next;
			}
			s = new section();
			s.key = key;
			s.next = (section)sections[pos];
			sections[pos] = s;
			return s;
		}
		
		private class section
		{
			public Template tpl;
			public string key;
			public node preceding;
			public section next;
			public section nextSection;
		}
	
		/*
		 *	Internal data types
		 */
	
		private class node
		{
			public cell val;
			public node next;
		}
		
		private class cell
		{
			public cell()
			{
				shared = false;
			}
			public string val;
			public bool shared;
		}
		
		private class field : cell
		{
			public field(string Key)
			{
				key = Key;
				val = "";
				shared = true;
			}
			public string key;
			public field next;
		}
	
		private node head;
		private node addedHead;
		private node addedTail;
		private Object[] fields;
		private Object[] sections;
		private section firstSection;
		private Template tpl;
		private Template parent;
	}

} // Namespace ends