using System.Collections.Generic;

namespace Mail.Contracts;

public static class Templates
{
    private const string VestfoldFylkeStyleTemplate = @"<!doctype html>
       <html lang=""no"">
       <head>
           <meta http-equiv=""content-type"" content=""text/html; charset=utf-8"" />
           <style>
             body { 
               font-family: Calibri, Arial, sans-serif;
               font-size: 12pt;
             }
             a { 
               color: #005260;
             }
             #vfk_signature { 
               font-family: Calibri, Arial, sans-serif;
               font-size: 11pt;
             }
             #vfk_signature a { 
               color: #005260;
               text-decoration: none;
             }
             #vfk_person a { 
               color: #000000;
             }
           </style>
       </head>
       <body>
       <div id=""content"">
           {{{body}}}
       </div>
       <br />
       <br />
       <div id=""vfk_signature"">
         <div id=""vfk_person"">
           Vennlig hilsen<br />
           <br />
           <b>{{signature.name}}</b><br />
           
           {{#if signature.title}}
            {{signature.title}}<br />
           {{/if}}
           
           {{#if signature.department}}
            {{signature.department}}<br/>
           {{/if}}
           
           {{#if signature.company}}
            {{signature.company}}<br/>
           {{/if}}
           {{#if signature.phone}}
               <br/>
               Telefon: <a href=""tel:{{signature.phone}}"">{{signature.phone}}</a>
               
               {{#if signature.mobile}}
                   / 
               {{/if}}
           {{/if}}
           {{#if signature.mobile}}
               {{#unless signature.phone}}
                   <br/>
               {{/unless}}
               
               Mobil: <a href=""tel:{{signature.mobile}}"">{{signature.mobile}}</a>
           {{/if}}
         </div>
         <div id=""vfk_company"">
           <br>
           <img src=""https://logo.api.vestfoldfylke.no/logos/vestfold_fylkesvapen_epost.png"" alt=""Fylkesvåpen, Vestfold fylkeskommune""><br />
           <b>Vestfold fylkeskommune</b><br />
           
           {{#if signature.webpage}}
               <a href=""{{signature.webpage}}"">{{signature.webpage}}</a>
           {{else}}
               <a href=""https://www.vestfoldfylke.no/"">www.vestfoldfylke.no</a>
           {{/if}}
         </div>
       </div>
       </body>
       </html>";
    
    private const string TelemarkFylkeStyleTemplate = @"<!doctype html>
    <html lang=""no"">
    <head>
        <meta http-equiv=""content-type"" content=""text/html; charset=utf-8"" />
        <style>
          body { 
            font-family: Calibri, Arial, sans-serif;
            font-size: 12pt;
          }
          a { 
            color: #005260;
          }
          #tfk_signature { 
            font-family: Calibri, Arial, sans-serif;
            font-size: 11pt;
          }
          #tfk_signature a { 
            color: #005260;
            text-decoration: none;
          }
          #tfk_person a { 
            color: #000000;
          }
        </style>
    </head>
    <body>
    <div id=""content"">
        {{{body}}}
    </div>
    <br />
    <br />
    <div id=""tfk_signature"">
      <div id=""tfk_person"">
        Vennlig hilsen<br />
        <br />
        <b>{{signature.name}}</b><br />
        
        {{#if signature.title}}
            {{signature.title}}<br />
        {{/if}}
        
        {{#if signature.department}}
            {{signature.department}}<br/>
        {{/if}}
        
        {{#if signature.company}}
            {{signature.company}}<br/>
        {{/if}}
        {{#if signature.phone}}
            <br/>
            Telefon: <a href=""tel:{{signature.phone}}"">{{signature.phone}}</a>
            
            {{#if signature.mobile}}
                / 
            {{/if}}
        {{/if}}
        {{#if signature.mobile}}
            {{#unless signature.phone}}
                <br/>
            {{/unless}}
            
            Mobil: <a href=""tel:{{signature.mobile}}"">{{signature.mobile}}</a>
        {{/if}}
      </div>
      <div id=""tfk_company"">
        <br>
        <img src=""https://logo.api.telemarkfylke.no/logos/telemark_fylkesvapen_epost.png"" alt=""Fylkesvåpen, Telemark fylkeskommune""><br />
        <b>Telemark fylkeskommune</b><br />
        
        {{#if signature.webpage}}
            <a href=""{{signature.webpage}}"">{{signature.webpage}}</a>
        {{else}}
            <a href=""https://www.telemarkfylke.no/"">www.telemarkfylke.no</a>
        {{/if}}
      </div>
    </div>
    </body>
    </html>";
    
    public static Dictionary<string, string> AllTemplates { get; } = new()
    {
        { "vestfoldfylke", VestfoldFylkeStyleTemplate },
        { "telemarkfylke", TelemarkFylkeStyleTemplate }
    };
}