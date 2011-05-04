﻿#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2011
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

#region Usings

using System.Data;

using DotNetNuke.Entities.Content.Taxonomy;

#endregion

namespace DotNetNuke.Entities.Content.Data
{
	/// <summary>
	/// Interface of DataService.
	/// </summary>
	/// <seealso cref="DataService"/>
    public interface IDataService
    {
        //Content Item Methods
        int AddContentItem(ContentItem contentItem, int createdByUserId);

        void DeleteContentItem(ContentItem contentItem);

        IDataReader GetContentItem(int contentItemId);

        IDataReader GetContentItemsByTerm(string term);

        IDataReader GetUnIndexedContentItems();

        void UpdateContentItem(ContentItem contentItem, int lastModifiedByUserId);

        //Content MetaData Methods
        void AddMetaData(ContentItem contentItem, string name, string value);

        void DeleteMetaData(ContentItem contentItem, string name, string value);

        IDataReader GetMetaData(int contentItemId);

        //ContentType Methods
        int AddContentType(ContentType contentType);

        void DeleteContentType(ContentType contentType);

        IDataReader GetContentTypes();

        void UpdateContentType(ContentType contentType);

        //ScopeType Methods
        int AddScopeType(ScopeType scopeType);

        void DeleteScopeType(ScopeType scopeType);

        IDataReader GetScopeTypes();

        void UpdateScopeType(ScopeType scopeType);

        //Term Methods
        int AddHeirarchicalTerm(Term term, int createdByUserId);

        int AddSimpleTerm(Term term, int createdByUserId);

        void AddTermToContent(Term term, ContentItem contentItem);

        void DeleteSimpleTerm(Term term);

        void DeleteHeirarchicalTerm(Term term);

        IDataReader GetTerm(int termId);

        IDataReader GetTermsByContent(int contentItemId);

        IDataReader GetTermsByVocabulary(int vocabularyId);

        void RemoveTermsFromContent(ContentItem contentItem);

        void UpdateHeirarchicalTerm(Term term, int lastModifiedByUserId);

        void UpdateSimpleTerm(Term term, int lastModifiedByUserId);

        //Vocabulary Methods
        int AddVocabulary(Vocabulary vocabulary, int createdByUserId);

        void DeleteVocabulary(Vocabulary vocabulary);

        IDataReader GetVocabularies();

        void UpdateVocabulary(Vocabulary vocabulary, int lastModifiedByUserId);
    }
}