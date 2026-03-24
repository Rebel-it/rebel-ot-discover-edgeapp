import asyncio
from asyncua import Server, ua

async def main():
    server = Server()

    # MUST be awaited before creating nodes
    await server.init()

    await server.load_certificate("src/server_cert.der")
    await server.load_private_key("src/server_key.pem")


    server.set_endpoint("opc.tcp://127.0.0.1:53530/rebelit/server/")
    server.set_server_name("Rebel OPC UA Test Server")

    # Create namespace
    uri = "http://rebelit.nl"
    idx = await server.register_namespace(uri)

    # Create object
    objects = server.nodes.objects
    myobj = await objects.add_object(idx, "VariableTestObject")

    # Uint variables
    uintVariable16 = await myobj.add_variable(idx, "MyUInt16Variable", 65535, ua.VariantType.UInt16)
    uintVariable32 = await myobj.add_variable(idx, "MyUIntVariable", 123, ua.VariantType.UInt32)
    uintVariable64 = await myobj.add_variable(idx, "MyUInt64Variable", 12345678901234567890, ua.VariantType.UInt64)
    
    #int variables
    intVariable16 = await myobj.add_variable(idx, "MyInt16Variable", 32768, ua.VariantType.Int16)
    intVariable32 = await myobj.add_variable(idx, "MyIntVariable", 123, ua.VariantType.Int32)
    intVariable64 = await myobj.add_variable(idx, "MyInt64Variable", 1234567890123456789, ua.VariantType.Int64)

    # bool variable
    boolVariableTrue = await myobj.add_variable(idx, "MyBoolVariable", True, ua.VariantType.Boolean)
    boolVariableFalse = await myobj.add_variable(idx, "MyBoolVariableFalse", False, ua.VariantType.Boolean)

    #string variable
    stringVariable = await myobj.add_variable(idx, "MyStringVariable", "Hello, OPC UA!", ua.VariantType.String)

    #float variable
    floatVariable = await myobj.add_variable(idx, "MyFloatVariable", 3.14, ua.VariantType.Float)

    await uintVariable16.set_writable() 
    await uintVariable32.set_writable()
    await uintVariable64.set_writable()
    await intVariable16.set_writable()
    await intVariable32.set_writable()
    await intVariable64.set_writable()
    await boolVariableTrue.set_writable()
    await boolVariableFalse.set_writable()
    await stringVariable.set_writable()
    await floatVariable.set_writable()

    print("✅ Server running at: opc.tcp://127.0.0.1:53530/rebelit/server/")

    async with server:
        while True:
            await asyncio.sleep(1)

if __name__ == "__main__":
    asyncio.run(main())