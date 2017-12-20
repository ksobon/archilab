<Workspace Version="1.3.0.875" X="-210.682016962799" Y="-435.634975045793" zoom="1.34613730474456" ScaleFactor="1" Name="Home" Description="" RunType="Automatic" RunPeriod="1000" HasRunWithoutCrash="True">
  <NamespaceResolutionMap />
  <Elements>
    <archilabUI.ListSelector.ListSelector guid="3064f7d1-b6b2-424a-a877-3428ea6bd3a3" type="archilabUI.ListSelector.ListSelector" nickname="List Selector" x="521.156430033964" y="362.002360156702" isVisible="true" isUpstreamVisible="true" lacing="Disabled" isSelectedInput="False" IsFrozen="false" isPinned="false">
      <PortInfo index="0" default="False" />
      <listWrapper_name name0="1" name1="2" name2="3" name3="4" name4="5" name5="6" name6="7" name7="8" name8="9" name9="10" />
      <listWrapper_selected selected0="True" selected1="True" selected2="False" selected3="True" selected4="False" selected5="False" selected6="False" selected7="True" selected8="True" selected9="False" />
      <listWrapper_index index0="0" index1="1" index2="2" index3="3" index4="4" index5="5" index6="6" index7="7" index8="8" index9="9" />
    </archilabUI.ListSelector.ListSelector>
    <Dynamo.Graph.Nodes.CodeBlockNodeModel guid="bde97d4e-cec7-4275-93a6-2d4aae724254" type="Dynamo.Graph.Nodes.CodeBlockNodeModel" nickname="Code Block" x="321" y="366" isVisible="true" isUpstreamVisible="true" lacing="Disabled" isSelectedInput="False" IsFrozen="false" isPinned="false" CodeText="1..10..1;" ShouldFocus="false" />
    <CoreNodeModels.Watch guid="9bc4573c-a85f-4c20-b7b5-923d3376596d" type="CoreNodeModels.Watch" nickname="Watch" x="745.015982086192" y="361.596688265269" isVisible="true" isUpstreamVisible="true" lacing="Disabled" isSelectedInput="False" IsFrozen="false" isPinned="false">
      <PortInfo index="0" default="False" />
    </CoreNodeModels.Watch>
    <archilabUI.DropdownListSelector.DropdownListSelector guid="50458f5f-3727-4d81-94e9-5edf27f3baa3" type="archilabUI.DropdownListSelector.DropdownListSelector" nickname="Dropdown List Selector" x="500.764435664211" y="636.126128608204" isVisible="true" isUpstreamVisible="true" lacing="Disabled" isSelectedInput="False" IsFrozen="false" isPinned="false">
      <PortInfo index="0" default="False" />
      <listWrapper_name name0="1" name1="2" name2="3" name3="4" name4="5" name5="6" name6="7" name7="8" name8="9" name9="10" />
      <listWrapper_selected selected0="False" selected1="False" selected2="True" selected3="True" selected4="True" selected5="True" selected6="False" selected7="False" selected8="True" selected9="False" />
      <listWrapper_index index0="0" index1="1" index2="2" index3="3" index4="4" index5="5" index6="6" index7="7" index8="8" index9="9" />
    </archilabUI.DropdownListSelector.DropdownListSelector>
    <CoreNodeModels.Watch guid="9632df91-186e-4fcb-a006-ebcc7631fc82" type="CoreNodeModels.Watch" nickname="Watch" x="755.413732303016" y="637.180542930293" isVisible="true" isUpstreamVisible="true" lacing="Disabled" isSelectedInput="False" IsFrozen="false" isPinned="false">
      <PortInfo index="0" default="False" />
    </CoreNodeModels.Watch>
  </Elements>
  <Connectors>
    <Dynamo.Graph.Connectors.ConnectorModel start="3064f7d1-b6b2-424a-a877-3428ea6bd3a3" start_index="0" end="9bc4573c-a85f-4c20-b7b5-923d3376596d" end_index="0" portType="0" />
    <Dynamo.Graph.Connectors.ConnectorModel start="bde97d4e-cec7-4275-93a6-2d4aae724254" start_index="0" end="3064f7d1-b6b2-424a-a877-3428ea6bd3a3" end_index="0" portType="0" />
    <Dynamo.Graph.Connectors.ConnectorModel start="bde97d4e-cec7-4275-93a6-2d4aae724254" start_index="0" end="50458f5f-3727-4d81-94e9-5edf27f3baa3" end_index="0" portType="0" />
    <Dynamo.Graph.Connectors.ConnectorModel start="50458f5f-3727-4d81-94e9-5edf27f3baa3" start_index="0" end="9632df91-186e-4fcb-a006-ebcc7631fc82" end_index="0" portType="0" />
  </Connectors>
  <Notes />
  <Annotations />
  <Presets />
  <Cameras>
    <Camera Name="Background Preview" eyeX="-17" eyeY="24" eyeZ="50" lookX="12" lookY="-13" lookZ="-58" upX="0" upY="1" upZ="0" />
  </Cameras>
</Workspace>