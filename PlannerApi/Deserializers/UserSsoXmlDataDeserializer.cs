using System.Xml.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace events_planner.Deserializers {

    [XmlRoot(ElementName="attributes", Namespace="http://www.yale.edu/tp/cas")]
    public class Attributes {

        [XmlElement(ElementName="mail_ynov", Namespace="http://www.yale.edu/tp/cas")]
        public string Email { get; set; }

        [XmlElement(ElementName="IdExtern", Namespace="http://www.yale.edu/tp/cas")]
        public int? SSOID { get; set; }

        [XmlElement(ElementName="Prenom", Namespace="http://www.yale.edu/tp/cas")]
        public string FirstName { get; set; }

        [XmlElement(ElementName="Nom", Namespace="http://www.yale.edu/tp/cas")]
        public string LastName { get; set; }

        [XmlElement(ElementName="avatar", Namespace="http://www.yale.edu/tp/cas")]
        public string ImageUrl { get; set; }

        // PROMOTION:

        // Don't set as Property for reflection purpose in UserServices
        [XmlIgnore]
        public ProgramOrStaff EnumType;

        [XmlChoiceIdentifier("EnumType")]
        [XmlElement("etu_programme_txt", Namespace="http://www.yale.edu/tp/cas")]
        [XmlElement("staff_formation_txt", Namespace="http://www.yale.edu/tp/cas")]
        public string Name { get; set; }

        [XmlElement(ElementName = "etu_annee_txt", Namespace="http://www.yale.edu/tp/cas")]
        public string Year { get; set; } = DateTime.Now.Year.ToString();

        [XmlElement(ElementName = "etu_promotion_txt", Namespace="http://www.yale.edu/tp/cas")]
        public string Description { get; set; }

        // Don't set as Property for reflection purpose in UserServices
        [XmlElement("categories", Namespace = "http://www.yale.edu/tp/cas")]
        public string Categories;
    }

    [XmlRoot(ElementName="authenticationSuccess", Namespace="http://www.yale.edu/tp/cas")]
    public class AuthenticationSuccess {

        [XmlElement(ElementName="user", Namespace="http://www.yale.edu/tp/cas")]
        public string User { get; set; }

        [XmlElement(ElementName="attributes", Namespace="http://www.yale.edu/tp/cas")]
        public Attributes Attributes { get; set; }
    }

    [XmlRoot(ElementName = "authenticationFailure", Namespace = "http://www.yale.edu/tp/cas")]
    public class AuthenticationFailure { }

    [XmlRoot(ElementName="serviceResponse", Namespace="http://www.yale.edu/tp/cas")]
    public class ServiceResponse {

        [XmlElement(ElementName="authenticationSuccess", Namespace="http://www.yale.edu/tp/cas")]
        public AuthenticationSuccess AuthenticationSuccess { get; set; }

        [XmlElement(ElementName = "authenticationFailure", Namespace = "http://www.yale.edu/tp/cas")]
        public AuthenticationFailure AuthenticationFailure { get; set; } = null;

        [XmlAttribute(AttributeName="cas", Namespace="http://www.w3.org/2000/xmlns/")]
        public string Cas { get; set; }
    }

    [XmlType(IncludeInSchema = false)]
    public enum ProgramOrStaff {
        etu_programme_txt,
        staff_formation_txt

    }

}