using System.Xml.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace events_planner.Deserializers {
    
    [XmlRoot(ElementName="attributes", Namespace="http://www.yale.edu/tp/cas")]
    public class Attributes {
        /*
            Properties available in SSO but no used in User model. /!\
            
            [XmlElement(ElementName="LdapAuthenticationHandler.dn", Namespace="http://www.yale.edu/tp/cas")]
            public string LdapAuthenticationHandlerdn { get; set; }
                
            [XmlElement(ElementName="cn", Namespace="http://www.yale.edu/tp/cas")]
            public string FullName { get; set; }
        */
        
        [XmlElement(ElementName="sn", Namespace="http://www.yale.edu/tp/cas")]
        public string LastName { get; set; }
        
        [XmlElement(ElementName="mail", Namespace="http://www.yale.edu/tp/cas")]
        public string Email { get; set; }
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

}