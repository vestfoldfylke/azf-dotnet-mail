using System;
using System.ComponentModel;

namespace Mail.Contracts;

public class EventMessage
{
    [Description("The attempt number (starting from 0)")]
    public int Attempt { get; init; }
    
    [Description("The city in which the (click was generated | open was generated)")]
    public string? City { get; init; }
    
    [Description("The SMTP error code")]
    public string? Code { get; init; }
    
    [Description("The code (alpha-2) of the country in which the (click was generated | open was generated)")]
    public string? CountryCode { get; init; }
    
    [Description("The name of the country in which the (click was generated | open was generated)")]
    public string? CountryName { get; init; }
    
    [Description("Human readable description received over SMTP")]
    public string? Description { get; init; }
    
    [Description("The URL to which the clicker was directed to")]
    public string? Destination { get; init; }
    
    [Description("The envelope of the message")]
    public string? Envelope { get; init; }
    
    [Description("Describes which type of record it is: attempt|bounce|click|delivery|failure|open|response")]
    public required string Event { get; init; }
    
    [Description("The \"from\" IP address")]
    public string? From { get; init; }
    
    [Description("The headers that where used to make the call, separated by newlines (for click and open events)")]
    public string? Headers { get; init; }
    
    [Description("The message id")]
    public string? Id { get; init; }
    
    [Description("The IP address of the system (where the link was clicked | who requested the tracking picture)")]
    public string? Ip { get; init; }
    
    [Description("The mime content of a received bounce")]
    public string? Mime { get; init; }
    
    [Description("The properties for an attempt (preventscam, inlinecss, trackbounces, trackopens, and trackclicks) of the message (semicolon separated)")]
    public string? Properties { get; init; }
    
    [Description("The protocol of the request (e.g. http or https)")]
    public string? Protocol { get; init; }
    
    [Description("The recipient of the message")]
    public string? Recipient { get; init; }
    
    [Description("The code of the region in which the (click was generated | open was generated)")]
    public string? RegionCode { get; init; }
    
    [Description("SMTP status code (like \"5.0.0\")")]
    public string? Status { get; init; }
    
    [Description("State in the SMTP protocol during which the error occured")]
    public string? State { get; init; }
    
    [Description("The tags of the message (semicolon separated)")]
    public string? Tags { get; init; }
    
    [Description("The id of the template that is used (0 if no template is used or when it is not available)")]
    public int TemplateId { get; init; }
    
    [Description("The time of the event (YYYY-MM-DD hh:mm:ss formatted)")]
    public DateTime? Time { get; init; }
    
    [Description("The \"to\" IP address")]
    public string? To { get; init; }
    
    [Description("The result type")]
    public string? Type { get; init; }
    
    [Description("The URL in the message that was clicked")]
    public string? Url { get; init; }
}