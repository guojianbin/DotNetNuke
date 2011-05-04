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

using DotNetNuke.Data;
using DotNetNuke.Entities.Content.Taxonomy;

#endregion

namespace DotNetNuke.Entities.Content.Data
{
	/// <summary>
	/// Persistent data of content with DataProvider instance.
	/// </summary>
	/// <remarks>
	/// It's better to use Util.GetDataService instead of create new instance directly.
	/// </remarks>
	/// <example>
	/// <code lang="C#">
	/// public ContentController() : this(Util.GetDataService())
    /// {
    /// }
    /// public ContentController(IDataService dataService)
    /// {
    ///     _dataService = dataService;
    /// }
	/// </code>
	/// </example>
    public class DataService : IDataService
    {
        private readonly DataProvider provider = DataProvider.Instance();

        #region "ContentItem Methods"

		/// <summary>
		/// Adds the content item.
		/// </summary>
		/// <param name="contentItem">The content item.</param>
		/// <param name="createdByUserId">The created by user id.</param>
		/// <returns>conent item id.</returns>
        public int AddContentItem(ContentItem contentItem, int createdByUserId)
        {
            return provider.ExecuteScalar<int>("AddContentItem",
                                               contentItem.Content,
                                               contentItem.ContentTypeId,
                                               contentItem.TabID,
                                               contentItem.ModuleID,
                                               contentItem.ContentKey,
                                               contentItem.Indexed,
                                               createdByUserId);
        }

		/// <summary>
		/// Deletes the content item.
		/// </summary>
		/// <param name="contentItem">The content item.</param>
        public void DeleteContentItem(ContentItem contentItem)
        {
            provider.ExecuteNonQuery("DeleteContentItem", contentItem.ContentItemId);
        }

		/// <summary>
		/// Gets the content item.
		/// </summary>
		/// <param name="contentItemId">The content item id.</param>
		/// <returns>data reader.</returns>
        public IDataReader GetContentItem(int contentItemId)
        {
            return provider.ExecuteReader("GetContentItem", contentItemId);
        }

		/// <summary>
		/// Gets the content items by term.
		/// </summary>
		/// <param name="term">The term.</param>
		/// <returns>data reader.</returns>
        public IDataReader GetContentItemsByTerm(string term)
        {
            return provider.ExecuteReader("GetContentItemsByTerm", term);
        }

		/// <summary>
		/// Gets the un indexed content items.
		/// </summary>
		/// <returns>data reader.</returns>
        public IDataReader GetUnIndexedContentItems()
        {
            return provider.ExecuteReader("GetUnIndexedContentItems");
        }

		/// <summary>
		/// Updates the content item.
		/// </summary>
		/// <param name="contentItem">The content item.</param>
		/// <param name="createdByUserId">The created by user id.</param>
        public void UpdateContentItem(ContentItem contentItem, int createdByUserId)
        {
            provider.ExecuteNonQuery("UpdateContentItem",
                                     contentItem.ContentItemId,
                                     contentItem.Content,
                                     contentItem.ContentTypeId,
                                     contentItem.TabID,
                                     contentItem.ModuleID,
                                     contentItem.ContentKey,
                                     contentItem.Indexed,
                                     createdByUserId);
        }

        #endregion

        #region "MetaData Methods"

		/// <summary>
		/// Adds the meta data.
		/// </summary>
		/// <param name="contentItem">The content item.</param>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
        public void AddMetaData(ContentItem contentItem, string name, string value)
        {
            provider.ExecuteNonQuery("AddMetaData", contentItem.ContentItemId, name, value);
        }

		/// <summary>
		/// Deletes the meta data.
		/// </summary>
		/// <param name="contentItem">The content item.</param>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
        public void DeleteMetaData(ContentItem contentItem, string name, string value)
        {
            provider.ExecuteNonQuery("DeleteMetaData", contentItem.ContentItemId, name, value);
        }

		/// <summary>
		/// Gets the meta data.
		/// </summary>
		/// <param name="contentItemId">The content item id.</param>
		/// <returns>data reader.</returns>
        public IDataReader GetMetaData(int contentItemId)
        {
            return provider.ExecuteReader("GetMetaData", contentItemId);
        }

        #endregion

        #region "ContentType Methods"

		/// <summary>
		/// Adds the type of the content.
		/// </summary>
		/// <param name="contentType">Type of the content.</param>
		/// <returns>content type id.</returns>
        public int AddContentType(ContentType contentType)
        {
            return provider.ExecuteScalar<int>("AddContentType", contentType.ContentType);
        }

        public void DeleteContentType(ContentType contentType)
        {
            provider.ExecuteNonQuery("DeleteContentType", contentType.ContentTypeId);
        }

		/// <summary>
		/// Gets the content types.
		/// </summary>
		/// <returns>data reader.</returns>
        public IDataReader GetContentTypes()
        {
            return provider.ExecuteReader("GetContentTypes");
        }

		/// <summary>
		/// Updates the type of the content.
		/// </summary>
		/// <param name="contentType">Type of the content.</param>
        public void UpdateContentType(ContentType contentType)
        {
            provider.ExecuteNonQuery("UpdateContentType", contentType.ContentTypeId, contentType.ContentType);
        }

        #endregion

        #region "ScopeType Methods"

		/// <summary>
		/// Adds the type of the scope.
		/// </summary>
		/// <param name="scopeType">Type of the scope.</param>
		/// <returns>scope type id.</returns>
        public int AddScopeType(ScopeType scopeType)
        {
            return provider.ExecuteScalar<int>("AddScopeType", scopeType.Type);
        }

		/// <summary>
		/// Deletes the type of the scope.
		/// </summary>
		/// <param name="scopeType">Type of the scope.</param>
        public void DeleteScopeType(ScopeType scopeType)
        {
            provider.ExecuteNonQuery("DeleteScopeType", scopeType.ScopeTypeId);
        }

		/// <summary>
		/// Gets the scope types.
		/// </summary>
		/// <returns>data reader.</returns>
        public IDataReader GetScopeTypes()
        {
            return provider.ExecuteReader("GetScopeTypes");
        }

		/// <summary>
		/// Updates the type of the scope.
		/// </summary>
		/// <param name="scopeType">Type of the scope.</param>
        public void UpdateScopeType(ScopeType scopeType)
        {
            provider.ExecuteNonQuery("UpdateScopeType", scopeType.ScopeTypeId, scopeType.Type);
        }

        #endregion

        #region "Term Methods"

		/// <summary>
		/// Adds the heirarchical term.
		/// </summary>
		/// <param name="term">The term.</param>
		/// <param name="createdByUserId">The created by user id.</param>
		/// <returns>term id.</returns>
        public int AddHeirarchicalTerm(Term term, int createdByUserId)
        {
            return provider.ExecuteScalar<int>("AddHeirarchicalTerm", term.VocabularyId, term.ParentTermId, term.Name, term.Description, term.Weight, createdByUserId);
        }

		/// <summary>
		/// Adds the simple term.
		/// </summary>
		/// <param name="term">The term.</param>
		/// <param name="createdByUserId">The created by user id.</param>
		/// <returns>term id.</returns>
        public int AddSimpleTerm(Term term, int createdByUserId)
        {
            return provider.ExecuteScalar<int>("AddSimpleTerm", term.VocabularyId, term.Name, term.Description, term.Weight, createdByUserId);
        }

        public void AddTermToContent(Term term, ContentItem contentItem)
        {
            provider.ExecuteNonQuery("AddTermToContent", term.TermId, contentItem.ContentItemId);
        }

		/// <summary>
		/// Deletes the simple term.
		/// </summary>
		/// <param name="term">The term.</param>
        public void DeleteSimpleTerm(Term term)
        {
            provider.ExecuteNonQuery("DeleteSimpleTerm", term.TermId);
        }

		/// <summary>
		/// Deletes the heirarchical term.
		/// </summary>
		/// <param name="term">The term.</param>
        public void DeleteHeirarchicalTerm(Term term)
        {
            provider.ExecuteNonQuery("DeleteHeirarchicalTerm", term.TermId);
        }

		/// <summary>
		/// Gets the term.
		/// </summary>
		/// <param name="termId">The term id.</param>
		/// <returns>data reader.</returns>
        public IDataReader GetTerm(int termId)
        {
            return provider.ExecuteReader("GetTerm", termId);
        }

		/// <summary>
		/// Gets the content of the terms by.
		/// </summary>
		/// <param name="contentItemId">The content item id.</param>
		/// <returns>data reader.</returns>
        public IDataReader GetTermsByContent(int contentItemId)
        {
            return provider.ExecuteReader("GetTermsByContent", contentItemId);
        }

		/// <summary>
		/// Gets the terms by vocabulary.
		/// </summary>
		/// <param name="vocabularyId">The vocabulary id.</param>
		/// <returns>data reader.</returns>
        public IDataReader GetTermsByVocabulary(int vocabularyId)
        {
            return provider.ExecuteReader("GetTermsByVocabulary", vocabularyId);
        }

		/// <summary>
		/// Removes the content of the terms from.
		/// </summary>
		/// <param name="contentItem">The content item.</param>
        public void RemoveTermsFromContent(ContentItem contentItem)
        {
            provider.ExecuteNonQuery("RemoveTermsFromContent", contentItem.ContentItemId);
        }

		/// <summary>
		/// Updates the heirarchical term.
		/// </summary>
		/// <param name="term">The term.</param>
		/// <param name="lastModifiedByUserId">The last modified by user id.</param>
        public void UpdateHeirarchicalTerm(Term term, int lastModifiedByUserId)
        {
            provider.ExecuteNonQuery("UpdateHeirarchicalTerm", term.TermId, term.VocabularyId, term.ParentTermId, term.Name, term.Description, term.Weight, lastModifiedByUserId);
        }

		/// <summary>
		/// Updates the simple term.
		/// </summary>
		/// <param name="term">The term.</param>
		/// <param name="lastModifiedByUserId">The last modified by user id.</param>
        public void UpdateSimpleTerm(Term term, int lastModifiedByUserId)
        {
            provider.ExecuteNonQuery("UpdateSimpleTerm", term.TermId, term.VocabularyId, term.Name, term.Description, term.Weight, lastModifiedByUserId);
        }

        #endregion

        #region "Vocabulary Methods"

		/// <summary>
		/// Adds the vocabulary.
		/// </summary>
		/// <param name="vocabulary">The vocabulary.</param>
		/// <param name="createdByUserId">The created by user id.</param>
		/// <returns>Vocabulary id.</returns>
        public int AddVocabulary(Vocabulary vocabulary, int createdByUserId)
        {
            return provider.ExecuteScalar<int>("AddVocabulary",
                                               vocabulary.Type,
                                               vocabulary.Name,
                                               vocabulary.Description,
                                               vocabulary.Weight,
                                               provider.GetNull(vocabulary.ScopeId),
                                               vocabulary.ScopeTypeId,
                                               createdByUserId);
        }

		/// <summary>
		/// Deletes the vocabulary.
		/// </summary>
		/// <param name="vocabulary">The vocabulary.</param>
        public void DeleteVocabulary(Vocabulary vocabulary)
        {
            provider.ExecuteNonQuery("DeleteVocabulary", vocabulary.VocabularyId);
        }

		/// <summary>
		/// Gets the vocabularies.
		/// </summary>
		/// <returns>data reader.</returns>
        public IDataReader GetVocabularies()
        {
            return provider.ExecuteReader("GetVocabularies");
        }

		/// <summary>
		/// Updates the vocabulary.
		/// </summary>
		/// <param name="vocabulary">The vocabulary.</param>
		/// <param name="lastModifiedByUserId">The last modified by user id.</param>
        public void UpdateVocabulary(Vocabulary vocabulary, int lastModifiedByUserId)
        {
            provider.ExecuteNonQuery("UpdateVocabulary",
                                     vocabulary.VocabularyId,
                                     vocabulary.Type,
                                     vocabulary.Name,
                                     vocabulary.Description,
                                     vocabulary.Weight,
                                     vocabulary.ScopeId,
                                     vocabulary.ScopeTypeId,
                                     lastModifiedByUserId);
        }

        #endregion
    }
}