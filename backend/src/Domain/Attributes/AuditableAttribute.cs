using System;

namespace Crm.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class AuditableAttribute : Attribute
{
}
