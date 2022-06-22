using System.Xml;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.EvaluationUtils;

public class XdmReturnValue
{
    public static TReturn ConvertXmdReturnValue<TSelector, TReturn>(TSelector expression, ISequence rawResults,
        ExecutionParameters executionParameters)
    {
        var typeActions = new Dictionary<Type, Func<TReturn>>
        {
            {typeof(bool), () => (TReturn) (object) rawResults.GetEffectiveBooleanValue()},
            {
                typeof(string), () =>
                {
                    var allValues = Atomize.AtomizeSequence(rawResults, executionParameters).GetAllValues();
                    if (allValues.Length == 0) return (TReturn) (object) "";
                    throw new NotImplementedException();
                    // return (TReturn)(object)string.Join(' ',allValues.Select(value => TypeCasting.CastToType<string ,string>(value, ValueType.XsString).GetAs<string>(ValueType.XsString)));
                }
            },
            {typeof(XmlNode), () => (TReturn) (object) ((NodeValue)rawResults.First()!).Value()}
        };

        return new TypeSwitchCase<TReturn>(typeActions).Run(typeof(TReturn));
    }
    //     export default function convertXDMReturnValue<
    // 	TNode extends Node,
    // 	TReturnType extends keyof IReturnTypes<TNode>
    // >(
    // 	expression: EvaluableExpression | string,
    // 	rawResults: ISequence,
    // 	returnType: TReturnType,
    // 	executionParameters: ExecutionParameters
    // ): IReturnTypes<TNode>[TReturnType] {
    // 	switch (returnType) {
    // 		case ReturnType.BOOLEAN: {
    // 			const ebv = rawResults.getEffectiveBooleanValue();
    // 			return ebv as IReturnTypes<TNode>[TReturnType];
    // 		}
    //
    // 		case ReturnType.STRING: {
    // 			const allValues = atomize(rawResults, executionParameters).getAllValues();
    // 			if (!allValues.length) {
    // 				return '' as IReturnTypes<TNode>[TReturnType];
    // 			}
    // 			// Atomize to convert (attribute)nodes to be strings
    // 			return allValues
    // 				.map((value) => castToType(value, ValueType.XSSTRING).value)
    // 				.join(' ') as IReturnTypes<TNode>[TReturnType];
    // 		}
    // 		case ReturnType.STRINGS: {
    // 			const allValues = atomize(rawResults, executionParameters).getAllValues();
    // 			if (!allValues.length) {
    // 				return [] as IReturnTypes<TNode>[TReturnType];
    // 			}
    // 			// Atomize all parts
    // 			return allValues.map((value) => {
    // 				return value.value + '';
    // 			}) as IReturnTypes<TNode>[TReturnType];
    // 		}
    //
    // 		case ReturnType.NUMBER: {
    // 			const first = rawResults.first();
    // 			if (first === null) {
    // 				return NaN as IReturnTypes<TNode>[TReturnType];
    // 			}
    // 			if (!isSubtypeOf(first.type, ValueType.XSNUMERIC)) {
    // 				return NaN as IReturnTypes<TNode>[TReturnType];
    // 			}
    // 			return first.value as IReturnTypes<TNode>[TReturnType];
    // 		}
    //
    // 		case ReturnType.FIRST_NODE: {
    // 			const first = rawResults.first();
    // 			if (first === null) {
    // 				return null as IReturnTypes<TNode>[TReturnType];
    // 			}
    // 			if (!isSubtypeOf(first.type, ValueType.NODE)) {
    // 				throw new Error(
    // 					'Expected XPath ' +
    // 						evaluableExpressionToString(expression) +
    // 						' to resolve to Node. Got ' +
    // 						valueTypeToString(first.type)
    // 				);
    // 			}
    // 			// over here: unravel pointers. if they point to actual nodes:return them. if they point
    // 			// to lightweights, really make them, if they point to clones, clone them etc
    //
    // 			return realizeDom(
    // 				first.value as NodePointer,
    // 				executionParameters,
    // 				false
    // 			) as IReturnTypes<TNode>[TReturnType];
    // 		}
    //
    // 		case ReturnType.NODES: {
    // 			const allResults = rawResults.getAllValues();
    //
    // 			if (
    // 				!allResults.every((value) => {
    // 					return isSubtypeOf(value.type, ValueType.NODE);
    // 				})
    // 			) {
    // 				throw new Error(
    // 					'Expected XPath ' +
    // 						evaluableExpressionToString(expression) +
    // 						' to resolve to a sequence of Nodes.'
    // 				);
    // 			}
    // 			return allResults.map((nodeValue) => {
    // 				return realizeDom(
    // 					nodeValue.value as NodePointer,
    // 					executionParameters,
    // 					false
    // 				) as unknown;
    // 			}) as IReturnTypes<TNode>[TReturnType];
    // 		}
    //
    // 		case ReturnType.MAP: {
    // 			const allValues = rawResults.getAllValues();
    //
    // 			if (allValues.length !== 1) {
    // 				throw new Error(
    // 					'Expected XPath ' +
    // 						evaluableExpressionToString(expression) +
    // 						' to resolve to a single map.'
    // 				);
    // 			}
    // 			const first = allValues[0];
    // 			if (!isSubtypeOf(first.type, ValueType.MAP)) {
    // 				throw new Error(
    // 					'Expected XPath ' +
    // 						evaluableExpressionToString(expression) +
    // 						' to resolve to a map'
    // 				);
    // 			}
    // 			const transformedMap = transformMapToObject(
    // 				first as MapValue,
    // 				executionParameters
    // 			).next(IterationHint.NONE);
    // 			return transformedMap.value as IReturnTypes<TNode>[TReturnType];
    // 		}
    //
    // 		case ReturnType.ARRAY: {
    // 			const allValues = rawResults.getAllValues();
    //
    // 			if (allValues.length !== 1) {
    // 				throw new Error(
    // 					'Expected XPath ' +
    // 						evaluableExpressionToString(expression) +
    // 						' to resolve to a single array.'
    // 				);
    // 			}
    // 			const first = allValues[0];
    // 			if (!isSubtypeOf(first.type, ValueType.ARRAY)) {
    // 				throw new Error(
    // 					'Expected XPath ' +
    // 						evaluableExpressionToString(expression) +
    // 						' to resolve to an array'
    // 				);
    // 			}
    // 			const transformedArray = transformArrayToArray(
    // 				first as ArrayValue,
    // 				executionParameters
    // 			).next(IterationHint.NONE);
    // 			return transformedArray.value as IReturnTypes<TNode>[TReturnType];
    // 		}
    //
    // 		case ReturnType.NUMBERS: {
    // 			const allValues = rawResults.getAllValues();
    // 			return allValues.map((value) => {
    // 				if (!isSubtypeOf(value.type, ValueType.XSNUMERIC)) {
    // 					throw new Error(
    // 						'Expected XPath ' +
    // 							evaluableExpressionToString(expression) +
    // 							' to resolve to numbers'
    // 					);
    // 				}
    // 				return value.value;
    // 			}) as IReturnTypes<TNode>[TReturnType];
    // 		}
    //
    // 		case ReturnType.ASYNC_ITERATOR: {
    // 			const it = rawResults.value;
    // 			let transformedValueGenerator: IIterator<Value> = null;
    // 			let done = false;
    // 			const getNextResult = () => {
    // 				while (!done) {
    // 					if (!transformedValueGenerator) {
    // 						const value = it.next(IterationHint.NONE);
    // 						if (value.done) {
    // 							done = true;
    // 							break;
    // 						}
    // 						transformedValueGenerator = transformXPathItemToJavascriptObject(
    // 							value.value,
    // 							executionParameters
    // 						);
    // 					}
    // 					const transformedValue = transformedValueGenerator.next(IterationHint.NONE);
    // 					transformedValueGenerator = null;
    // 					return transformedValue;
    // 				}
    // 				return Promise.resolve({
    // 					done: true,
    // 					value: null,
    // 				});
    // 			};
    // 			let toReturn: AsyncIterableIterator<any>;
    // 			if ('asyncIterator' in Symbol) {
    // 				toReturn = {
    // 					[Symbol.asyncIterator]() {
    // 						return this;
    // 					},
    // 					next: () =>
    // 						new Promise<IteratorResult<any>>((resolve) =>
    // 							resolve(getNextResult())
    // 						).catch((error) => {
    // 							printAndRethrowError(expression, error);
    // 						}),
    // 				};
    // 			} else {
    // 				toReturn = {
    // 					next: () => new Promise((resolve) => resolve(getNextResult())),
    // 				} as unknown as AsyncIterableIterator<any>;
    // 			}
    // 			return toReturn as IReturnTypes<TNode>[TReturnType];
    // 		}
    //
    // 		default: {
    // 			const allValues = rawResults.getAllValues();
    // 			const allValuesAreNodes = allValues.every((value) => {
    // 				return (
    // 					isSubtypeOf(value.type, ValueType.NODE) &&
    // 					!isSubtypeOf(value.type, ValueType.ATTRIBUTE)
    // 				);
    // 			});
    // 			if (allValuesAreNodes) {
    // 				const allResults = allValues.map((nodeValue) => {
    // 					return realizeDom(nodeValue.value as NodePointer, executionParameters, false);
    // 				}) as IReturnTypes<TNode>[TReturnType];
    //
    // 				if (allResults.length === 1) {
    // 					return allResults[0];
    // 				}
    // 				return allResults;
    // 			}
    // 			if (allValues.length === 1) {
    // 				const first = allValues[0];
    // 				if (isSubtypeOf(first.type, ValueType.ARRAY)) {
    // 					const transformedArray = transformArrayToArray(
    // 						first as ArrayValue,
    // 						executionParameters
    // 					).next(IterationHint.NONE);
    // 					return transformedArray.value as IReturnTypes<TNode>[TReturnType];
    // 				}
    // 				if (isSubtypeOf(first.type, ValueType.MAP)) {
    // 					const transformedMap = transformMapToObject(
    // 						first as MapValue,
    // 						executionParameters
    // 					).next(IterationHint.NONE);
    // 					return transformedMap.value as IReturnTypes<TNode>[TReturnType];
    // 				}
    // 				return atomizeSingleValue(first, executionParameters).first()
    // 					.value as IReturnTypes<TNode>[TReturnType];
    // 			}
    //
    // 			return atomize(sequenceFactory.create(allValues), executionParameters)
    // 				.getAllValues()
    // 				.map((atomizedValue) => {
    // 					return atomizedValue.value;
    // 				}) as IReturnTypes<TNode>[TReturnType];
    // 		}
    // 	}
    // }
}