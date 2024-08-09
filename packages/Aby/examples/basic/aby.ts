// deno-lint-ignore-file no-explicit-any no-unused-vars

/**
 * TODO
 * 
 * @param messagePrefix TODO
 * @returns TODO
 */
export function Bind(...messagePrefix: any[]): any
{
    return function (target: any, propertyKey: string, descriptor: PropertyDescriptor): PropertyDescriptor
    {
        const originalMethod = descriptor.value;
        descriptor.value = function (...args: any[])
        {
            const start = performance.now();
            const result = originalMethod.apply(this, args);
            const end = performance.now();
            
            console.log(`${messagePrefix}Execution time of ${propertyKey}: ${end - start} milliseconds`);
            
            return result;
        };
        
        return descriptor;
    };
}

/**
 * TODO
 * 
 * @param messagePrefix TODO
 * @returns TODO
 */
export function Query(messagePrefix: any): any
{
    return function (target: any, propertyKey: string, descriptor: PropertyDescriptor): PropertyDescriptor
    {
        const originalMethod = descriptor.value;
        descriptor.value = function (...args: any[])
        {
            const start = performance.now();
            const result = originalMethod.apply(this, args);
            const end = performance.now();
            
            console.log(`${messagePrefix}Execution time of ${propertyKey}: ${end - start} milliseconds`);
            
            return result;
        };
        
        return descriptor;
    };
}
