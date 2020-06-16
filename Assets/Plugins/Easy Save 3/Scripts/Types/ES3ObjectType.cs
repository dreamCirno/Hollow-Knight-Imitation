using System;
using UnityEngine;
using System.Collections;
using ES3Internal;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	public abstract class ES3ObjectType : ES3Type
	{
		public ES3ObjectType(Type type) : base(type) {}

		protected abstract void WriteObject(object obj, ES3Writer writer);
		protected abstract object ReadObject<T>(ES3Reader reader);

		protected virtual void ReadObject<T>(ES3Reader reader, object obj)
		{
			throw new NotSupportedException("ReadInto is not supported for type "+type);
		}

		public override void Write(object obj, ES3Writer writer)
		{
			if(!WriteUsingDerivedType(obj, writer))
				WriteObject(obj, writer);
		}

		public override object Read<T>(ES3Reader reader)
		{
			string propertyName;
			while(true)
			{
				propertyName = ReadPropertyName(reader);

				if(propertyName == ES3Type.typeFieldName)
					return ES3TypeMgr.GetOrCreateES3Type(reader.ReadType()).Read<T>(reader);
				else if(propertyName == null)
					return null;
				else
				{
					reader.overridePropertiesName = propertyName;
					return ReadObject<T>(reader);
				}
			}
		}

		public override void ReadInto<T>(ES3Reader reader, object obj)
		{
			string propertyName;
			while(true)
			{
				propertyName = ReadPropertyName(reader);

				if(propertyName == ES3Type.typeFieldName)
				{
					ES3TypeMgr.GetOrCreateES3Type(reader.ReadType()).ReadInto<T>(reader, obj);
					return;
				}
                // This is important we return if the enumerator returns null, otherwise we will encounter an endless cycle.
                else if (propertyName == null)
					return;
				else
				{
					reader.overridePropertiesName = propertyName;
					ReadObject<T>(reader, obj);
				}
			}
		}
	}
}