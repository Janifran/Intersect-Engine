﻿using System;
using Intersect.Localization;
using JetBrains.Annotations;

namespace Intersect.Server.Core.CommandParsing.Arguments
{
    public delegate bool ArgumentRequiredPredicate(ParserContext parserContext);

    public abstract class CommandArgument<TValue> : ICommandArgument
    {
        public char ShortName => Localization.ShortName;

        public string Name => Localization.Name;

        public Type ValueType => typeof(TValue);

        public object ValueTypeDefault => default(TValue);

        public object DefaultValue => default(TValue);

        public virtual bool IsFlag => false;

        public virtual bool AllowsMultiple => false;

        public virtual bool IsCollection => false;

        public string Delimeter { get; protected set; }

        private readonly bool mRequired;

        private readonly ArgumentRequiredPredicate mRequiredPredicate;

        public bool IsRequired(ParserContext parserContext) => mRequiredPredicate?.Invoke(parserContext) ?? mRequired;

        public bool IsPositional { get; }

        [NotNull]
        public LocaleArgument Localization { get; }

        protected CommandArgument(
            [NotNull] LocaleArgument localization,
            bool required = false,
            bool positional = false
        )
        {
            Localization = localization;
            IsPositional = positional;
            mRequired = required;
        }

        protected CommandArgument(
            [NotNull] LocaleArgument localization,
            [NotNull] ArgumentRequiredPredicate requiredPredicate,
            bool positional = false
        )
        {
            Localization = localization;
            IsPositional = positional;
            mRequiredPredicate = requiredPredicate;
        }

        public TValue DefaultValueAsType()
        {
            return DefaultValueAsType<TValue>();
        }

        public TDefaultValue DefaultValueAsType<TDefaultValue>()
        {
            return DefaultValue == null ? default(TDefaultValue) : (TDefaultValue) DefaultValue;
        }
    }

    public abstract class ArrayCommandArgument<TValue> : CommandArgument<TValue>
    {
        public int Count { get; }

        public override bool AllowsMultiple => Count > 1;

        public override bool IsCollection => true;

        protected ArrayCommandArgument(
            [NotNull] LocaleArgument localization,
            int count,
            [CanBeNull] string delimeter = null
        ) : base(localization)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            Count = count;
            Delimeter = delimeter;
        }
    }

    public abstract class CommandArgument : CommandArgument<object>
    {
        protected CommandArgument([NotNull] LocaleArgument localization)
            : base(localization)
        {
        }
    }
}